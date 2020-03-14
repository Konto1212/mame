// copyright-holders:K.Ito
#ifndef MAME_SOUND_MT32
#define MAME_SOUND_MT32

#include "..\munt\mt32emu\src\c_interface\c_interface.h"

#pragma once

//**************************************************************************
//  TYPE DEFINITIONS
//**************************************************************************

class mt32_device : public device_t,
	public device_sound_interface
{
public:
	mt32_device(const machine_config &mconfig, const char *tag, device_t *owner, uint32_t clock);

	/** Enqueues a single short MIDI message to be processed ASAP. The message must contain a status byte. */
	void play_msg(mt32emu_bit32u msg)
	{
		mt32emu_play_msg(context, msg);
	}
	/** Enqueues a single well formed System Exclusive MIDI message to be processed ASAP. */
	void play_sysex(const mt32emu_bit8u *sysex, mt32emu_bit32u len)
	{
		mt32emu_play_sysex(context, sysex, len);
	}

	void set_enable(int enable);

protected:
	// device-level overrides
	virtual void device_start() override;

	// sound stream update overrides
	virtual void sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples) override;

public:
	DECLARE_WRITE_LINE_MEMBER(set_state);   // enable/disable sound output

private:
	sound_stream *m_stream;   /* stream number */
	int m_frequency;          /* set frequency - this can be changed using the appropriate function */
	mt32emu_context context;
};

DECLARE_DEVICE_TYPE(MT32, mt32_device)

#endif
