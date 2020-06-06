// license:BSD-3-Clause
// copyright-holders:Peter Trauner
/*
  copyright peter trauner

  based on michael schwend's sid play

  Noise generation algorithm is used courtesy of Asger Alstrup Nielsen.
  His original publication can be found on the SID home page.

  Noise table optimization proposed by Phillip Wooller. The output of
  each table does not differ.

  MOS-8580 R5 combined waveforms recorded by Dennis "Deadman" Lindroos.
*/

#include "emu.h"
#include "sid.h"

#include "sidenvel.h"


namespace {

std::unique_ptr<float[]> filterTable;
std::unique_ptr<float[]> bandPassParam;
#define lowPassParam filterTable
float filterResTable[16];

#define maxLogicalVoices 4

const int mix16monoMiddleIndex = 256*maxLogicalVoices/2;
uint16_t mix16mono[256*maxLogicalVoices];

uint16_t zero16bit = 0;  /* either signed or unsigned */
//uint32_t splitBufferLen;

void MixerInit(int threeVoiceAmplify)
{
	long si;
	uint16_t ui;
	long ampDiv = maxLogicalVoices;

	if (threeVoiceAmplify)
	{
		ampDiv = (maxLogicalVoices-1);
	}

	/* Mixing formulas are optimized by sample input value. */

	si = (-128*maxLogicalVoices) * 256;
	for (ui = 0; ui < sizeof(mix16mono)/sizeof(uint16_t); ui++)
	{
		mix16mono[ui] = (uint16_t)(si/ampDiv) + zero16bit;
		si+=256;
	}

}

} // anonymous namespace


inline void SID6581_t::syncEm()
{
	bool sync[3];
	for (int v = 0; v < max_voices; v++)
	{
		sync[v] = optr[v].modulator->cycleLenCount <= 0;
		optr[v].cycleLenCount--;
	}

	for (int v = 0; v < max_voices; v++)
	{
		if (optr[v].sync && sync[v])
		{
			optr[v].cycleLenCount = 0;
			optr[v].outProc = &sidOperator::wave_calc_normal;
#if defined(DIRECT_FIXPOINT)
			optr[v].waveStep.l = 0;
#else
			optr[v].waveStep = optr[v].waveStepPnt = 0;
#endif
		}
	}
}


void SID6581_t::fill_buffer(stream_sample_t *buffer, stream_sample_t *buffer2, uint32_t bufferLen)
{
//void* SID6581_t::fill16bitMono(void* buffer, uint32_t numberOfSamples)

	for (; bufferLen > 0; bufferLen--)
	{
		int16_t data = (int16_t) mix16mono[unsigned(mix16monoMiddleIndex
								+(*optr[0].outProc)(&optr[0])
								+(*optr[1].outProc)(&optr[1])
								+(optr[2].outProc(&optr[2])&optr3_outputmask)
/* hack for digi sounds
   does n't seam to come from a tone operator
   ghostbusters and goldrunner everything except volume zeroed */
							+(masterVolume<<2)	//memidimemo To avoie low-freq noise
//                        +(*sampleEmuRout)()
		)];
		*buffer++ = data;
		*buffer2++ = data;

		syncEm();
	}
}

/* --------------------------------------------------------------------- Init */


/* Reset. */

bool SID6581_t::reset()
{
	for (int v = 0; v < max_voices; v++)
	{
		optr[v].clear();
		enveEmuResetOperator(&optr[v]);
	}
	optr3_outputmask = ~0;  /* on */

	//sampleEmuReset();

	filter.Type = filter.CurType = 0;
	filter.Value = 0;
	filter.Dy = filter.ResDy = 0;

	for (int v = 0; v < max_voices; v++)
	{
		optr[v].set();
		optr[v].set2();
	}

	return true;
}

void SID6581_t::postload()
{
	for (int v = 0; v < max_voices; v++)
	{
		optr[v].set();
		optr[v].set2();
	}
}


static void filterTableInit(running_machine &machine)
{
	int sample_rate = machine.sample_rate();
	uint16_t uk;
	/* Parameter calculation has not been moved to a separate function */
	/* by purpose. */
	const float filterRefFreq = 44100.0f;

	float yMax = 1.0f;
	float yMin = 0.01f;
	float yAdd;
	float yTmp, rk, rk2;

	float resDyMax;
	float resDyMin;
	float resDy;

	filterTable = std::make_unique<float[]>(0x800);
	bandPassParam = std::make_unique<float[]>(0x800);

	uk = 0;
	for (rk = 0; rk < 0x800; rk++)
	{
		filterTable[uk] = (((expf(rk/0x800*logf(400.0f))/60.0f)+0.05f)
			*filterRefFreq) / sample_rate;
		if (filterTable[uk] < yMin)
			filterTable[uk] = yMin;
		if (filterTable[uk] > yMax)
			filterTable[uk] = yMax;
		uk++;
	}

	/*extern float bandPassParam[0x800]; */
	yMax = 0.22f;
	yMin = 0.05f;  /* less for some R1/R4 chips */
	yAdd = (yMax-yMin)/2048.0f;
	yTmp = yMin;
	uk = 0;
	/* Some C++ compilers still have non-local scope! */
	for (rk2 = 0; rk2 < 0x800; rk2++)
	{
		bandPassParam[uk] = (yTmp*filterRefFreq) / sample_rate;
		yTmp += yAdd;
		uk++;
	}

	/*extern float filterResTable[16]; */
	resDyMax = 1.0f;
	resDyMin = 2.0f;
	resDy = resDyMin;
	for (uk = 0; uk < 16; uk++)
	{
		filterResTable[uk] = resDy;
		resDy -= ((resDyMin - resDyMax) / 15);
	}
	filterResTable[0] = resDyMin;
	filterResTable[15] = resDyMax;
}

void SID6581_t::init()
{
	for (int v = 0; v < max_voices; v++)
	{
		optr[v].sid = this;

		int mod_voi = (v + max_voices - 1) % max_voices;
		optr[v].modulator = &optr[mod_voi];
		optr[mod_voi].carrier = &optr[v];
		optr[v].filtVoiceMask = 1 << v;
	}

	PCMsid = uint32_t(PCMfreq * (16777216.0 / clock));
	PCMsidNoise = uint32_t((clock * 256.0) / PCMfreq);

	filter.Enabled = true;

	sidInitMixerEngine(device->machine());
	filterTableInit(device->machine());

	sidInitWaveformTables(type);

	enveEmuInit(PCMfreq, true);

	MixerInit(0);

	reset();
}


void SID6581_t::port_w(int offset, int data)
{
	offset &= 0x1f;
	switch (offset)
	{
	case 0x19:
	case 0x1a:
	case 0x1b:
	case 0x1c:
	case 0x1d:
	case 0x1e:
	case 0x1f:
		break;

	case 0x15:
	case 0x16:
	case 0x17:
	case 0x18:
		mixer_channel->update();
		reg[offset] = data;
		masterVolume = reg[0x18] & 15;
		masterVolumeAmplIndex = masterVolume << 8;

		if ((reg[0x18] & 0x80) && !(reg[0x17] & optr[2].filtVoiceMask))
			optr3_outputmask = 0;     /* off */
		else
			optr3_outputmask = ~0;  /* on */

		filter.Type = reg[0x18] & 0x70;
		if (filter.Type != filter.CurType)
		{
			filter.CurType = filter.Type;
			for (int v = 0; v < max_voices; v++)
				optr[v].filtLow = optr[v].filtRef = 0;
		}
		if (filter.Enabled)
		{
			filter.Value = 0x7ff & ((reg[0x15] & 7) | (uint16_t(reg[0x16]) << 3));
			if (filter.Type == 0x20)
				filter.Dy = bandPassParam ? bandPassParam[filter.Value] : 0.0f;
			else
				filter.Dy = lowPassParam ? lowPassParam[filter.Value] : 0.0f;
			filter.ResDy = filterResTable[reg[0x17] >> 4] - filter.Dy;
			if (filter.ResDy < 1.0f)
				filter.ResDy = 1.0f;
		}

		for (int v = 0; v < max_voices; v++)
		{
			optr[v].set();

			// relies on sidEmuSet also for other channels!
			optr[v].set2();
		}
		break;

	default:
		mixer_channel->update();
		reg[offset] = data;

		if (offset < 7)
			optr[0].reg[offset] = data;
		else if (offset < 14)
			optr[1].reg[offset - 7] = data;
		else if (offset < 21)
			optr[2].reg[offset - 14] = data;

		for (int v = 0; v < max_voices; v++)
		{
			optr[v].set();

			// relies on sidEmuSet also for other channels!
			optr[v].set2();
		}
		break;
	}
}


int SID6581_t::port_r(running_machine &machine, int offset)
{
	/* SIDPLAY reads last written at a sid address value */
	int data;
	offset &= 0x1f;
	switch (offset)
	{
	case 0x1d:
	case 0x1e:
	case 0x1f:
		data = 0xff;
		break;
	case 0x1b:
		mixer_channel->update();
		data = optr[2].output;
		break;
	case 0x1c:
		mixer_channel->update();
		data = optr[2].enveVol;
		break;
	default:
		data = reg[offset];
	}
	return data;
}
