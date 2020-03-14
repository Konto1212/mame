// copyright-holders:K.Ito
/***************************************************************************



****************************************************************************/

#include "emu.h"
#include "sound/mt32.h"
#include "..\mt32emu\src\c_interface\c_interface.h"

// device type definition
DEFINE_DEVICE_TYPE(MT32, mt32_device, "mt32", "MT32")


//**************************************************************************
//  LIVE DEVICE
//**************************************************************************

//-------------------------------------------------
//  beep_device - constructor
//-------------------------------------------------

mt32_device::mt32_device(const machine_config &mconfig, const char *tag, device_t *owner, uint32_t clock)
	: device_t(mconfig, MT32, tag, owner, clock)
	, device_sound_interface(mconfig, *this)
	, m_stream(nullptr)
	, m_frequency(clock)
{

}

void mt32_device::set_enable(int enable)
{
	if (m_enable != enable)
	{
		m_enable = enable;

		if (m_enable != 0)
		{
			mt32emu_open_synth(context);
		}
		else
		{
			mt32emu_close_synth(context);
		}
	}
}

//-------------------------------------------------
//  device_start - device-specific startup
//-------------------------------------------------

void mt32_device::device_start()
{
	m_stream = stream_alloc(0, 2, m_frequency);
	m_enable = 0;

	mt32emu_report_handler_i a;
	a.v0 = NULL;
	context = mt32emu_create_context(a, NULL);

	mt32emu_set_stereo_output_samplerate(context, m_frequency);
	mt32emu_set_analog_output_mode(context, mt32emu_analog_output_mode::MT32EMU_AOM_ACCURATE);
	mt32emu_set_samplerate_conversion_quality(context, mt32emu_samplerate_conversion_quality::MT32EMU_SRCQ_BEST);
	mt32emu_select_renderer_type(context, mt32emu_renderer_type::MT32EMU_RT_BIT16S);
	mt32emu_is_nice_partial_mixing_enabled(context);
	mt32emu_preallocate_reverb_memory(context, mt32emu_boolean::MT32EMU_BOOL_TRUE);

	mt32emu_add_rom_file(context, "MT32_CONTROL.ROM");
	mt32emu_add_rom_file(context, "MT32_PCM.ROM");
}

//-------------------------------------------------
//  sound_stream_update - handle a stream update
//-------------------------------------------------

void mt32_device::sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples)
{
	stream_sample_t *buffer1 = outputs[0];
	stream_sample_t *buffer2 = outputs[1];

	if (!m_enable)
	{
		memset(buffer1, 0, samples * sizeof(*buffer1));
		memset(buffer2, 0, samples * sizeof(*buffer2));
		return;
	}

	mt32emu_bit16s *buf = (mt32emu_bit16s*)malloc(sizeof(mt32emu_bit16s) * samples * 2);

	mt32emu_render_bit16s(context, buf, samples);

	mt32emu_bit16s *ptr = buf;
	while (samples-- > 0)
	{
		*buffer1++ = ((stream_sample_t)*ptr++);
		*buffer2++ = ((stream_sample_t)*ptr++);
	}

	free(buf);
}


//-------------------------------------------------
//  changing state to on from off will restart tone
//-------------------------------------------------

WRITE_LINE_MEMBER(mt32_device::set_state)
{
	/* only update if new state is not the same as old state */
	int on = (state) ? 1 : 0;
	if (m_enable == on)
		return;

	m_stream->update();
	set_enable(on);
}
