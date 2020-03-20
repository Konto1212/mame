/* Copyright (C) 2003, 2004, 2005, 2006, 2008, 2009 Dean Beeler, Jerome Fisher
 * Copyright (C) 2011-2019 Dean Beeler, Jerome Fisher, Sergey V. Mikayev
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 2.1 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#include <cstddef>

#include "emu.h"
#include "cm32p.h"

#include "cm32p_BReverbModel.h"

 // Analysing of state of reverb RAM address lines gives exact sizes of the buffers of filters used. This also indicates that
 // the reverb model implemented in the real devices consists of three series allpass filters preceded by a non-feedback comb (or a delay with a LPF)
 // and followed by three parallel comb filters

 // Because LA-32 chip makes it's output available to process by the Boss chip with a significant delay,
 // the Boss chip puts to the buffer the LA32 dry output when it is ready and performs processing of the _previously_ latched data.
 // Of course, the right way would be to use a dedicated variable for this, but our reverb model is way higher level,
 // so we can simply increase the input buffer size.
static const u32 PROCESS_DELAY = 1;

static const u32 MODE_3_ADDITIONAL_DELAY = 1;
static const u32 MODE_3_FEEDBACK_DELAY = 1;

// Avoid denormals degrading performance, using biased input
static const float BIAS = 1e-20f;

struct BReverbSettings {
	const u32 numberOfAllpasses;
	const u32 * const allpassSizes;
	const u32 numberOfCombs;
	const u32 * const combSizes;
	const u32 * const outLPositions;
	const u32 * const outRPositions;
	const u8 * const filterFactors;
	const u8 * const feedbackFactors;
	const u8 * const dryAmps;
	const u8 * const wetLevels;
	const u8 lpfAmp;
};

// Default reverb settings for "new" reverb model implemented in CM-32L / LAPC-I.
// Found by tracing reverb RAM data lines (thanks go to Lord_Nightmare & balrog).
static const BReverbSettings &getCM32L_LAPCSettings(const ReverbMode mode) {
	static const u32 MODE_0_NUMBER_OF_ALLPASSES = 3;
	static const u32 MODE_0_ALLPASSES[] = { 994, 729, 78 };
	static const u32 MODE_0_NUMBER_OF_COMBS = 4; // Well, actually there are 3 comb filters, but the entrance LPF + delay can be processed via a hacked comb.
	static const u32 MODE_0_COMBS[] = { 705 + PROCESS_DELAY, 2349, 2839, 3632 };
	static const u32 MODE_0_OUTL[] = { 2349, 141, 1960 };
	static const u32 MODE_0_OUTR[] = { 1174, 1570, 145 };
	static const u8  MODE_0_COMB_FACTOR[] = { 0xA0, 0x60, 0x60, 0x60 };
	static const u8  MODE_0_COMB_FEEDBACK[] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98 };
	static const u8  MODE_0_DRY_AMP[] = { 0xA0, 0xA0, 0xA0, 0xA0, 0xB0, 0xB0, 0xB0, 0xD0 };
	static const u8  MODE_0_WET_AMP[] = { 0x10, 0x30, 0x50, 0x70, 0x90, 0xC0, 0xF0, 0xF0 };
	static const u8  MODE_0_LPF_AMP = 0x60;

	static const u32 MODE_1_NUMBER_OF_ALLPASSES = 3;
	static const u32 MODE_1_ALLPASSES[] = { 1324, 809, 176 };
	static const u32 MODE_1_NUMBER_OF_COMBS = 4; // Same as for mode 0 above
	static const u32 MODE_1_COMBS[] = { 961 + PROCESS_DELAY, 2619, 3545, 4519 };
	static const u32 MODE_1_OUTL[] = { 2618, 1760, 4518 };
	static const u32 MODE_1_OUTR[] = { 1300, 3532, 2274 };
	static const u8  MODE_1_COMB_FACTOR[] = { 0x80, 0x60, 0x60, 0x60 };
	static const u8  MODE_1_COMB_FEEDBACK[] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
												  0x28, 0x48, 0x60, 0x70, 0x78, 0x80, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98 };
	static const u8  MODE_1_DRY_AMP[] = { 0xA0, 0xA0, 0xB0, 0xB0, 0xB0, 0xB0, 0xB0, 0xE0 };
	static const u8  MODE_1_WET_AMP[] = { 0x10, 0x30, 0x50, 0x70, 0x90, 0xC0, 0xF0, 0xF0 };
	static const u8  MODE_1_LPF_AMP = 0x60;

	static const u32 MODE_2_NUMBER_OF_ALLPASSES = 3;
	static const u32 MODE_2_ALLPASSES[] = { 969, 644, 157 };
	static const u32 MODE_2_NUMBER_OF_COMBS = 4; // Same as for mode 0 above
	static const u32 MODE_2_COMBS[] = { 116 + PROCESS_DELAY, 2259, 2839, 3539 };
	static const u32 MODE_2_OUTL[] = { 2259, 718, 1769 };
	static const u32 MODE_2_OUTR[] = { 1136, 2128, 1 };
	static const u8  MODE_2_COMB_FACTOR[] = { 0, 0x20, 0x20, 0x20 };
	static const u8  MODE_2_COMB_FEEDBACK[] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
												  0x30, 0x58, 0x78, 0x88, 0xA0, 0xB8, 0xC0, 0xD0,
												  0x30, 0x58, 0x78, 0x88, 0xA0, 0xB8, 0xC0, 0xD0,
												  0x30, 0x58, 0x78, 0x88, 0xA0, 0xB8, 0xC0, 0xD0 };
	static const u8  MODE_2_DRY_AMP[] = { 0xA0, 0xA0, 0xB0, 0xB0, 0xB0, 0xB0, 0xC0, 0xE0 };
	static const u8  MODE_2_WET_AMP[] = { 0x10, 0x30, 0x50, 0x70, 0x90, 0xC0, 0xF0, 0xF0 };
	static const u8  MODE_2_LPF_AMP = 0x80;

	static const u32 MODE_3_NUMBER_OF_ALLPASSES = 0;
	static const u32 MODE_3_NUMBER_OF_COMBS = 1;
	static const u32 MODE_3_DELAY[] = { 16000 + MODE_3_FEEDBACK_DELAY + PROCESS_DELAY + MODE_3_ADDITIONAL_DELAY };
	static const u32 MODE_3_OUTL[] = { 400, 624, 960, 1488, 2256, 3472, 5280, 8000 };
	static const u32 MODE_3_OUTR[] = { 800, 1248, 1920, 2976, 4512, 6944, 10560, 16000 };
	static const u8  MODE_3_COMB_FACTOR[] = { 0x68 };
	static const u8  MODE_3_COMB_FEEDBACK[] = { 0x68, 0x60 };
	static const u8  MODE_3_DRY_AMP[] = { 0x20, 0x50, 0x50, 0x50, 0x50, 0x50, 0x50, 0x50,
											0x20, 0x50, 0x50, 0x50, 0x50, 0x50, 0x50, 0x50 };
	static const u8  MODE_3_WET_AMP[] = { 0x18, 0x18, 0x28, 0x40, 0x60, 0x80, 0xA8, 0xF8 };

	static const BReverbSettings REVERB_MODE_0_SETTINGS = { MODE_0_NUMBER_OF_ALLPASSES, MODE_0_ALLPASSES, MODE_0_NUMBER_OF_COMBS, MODE_0_COMBS, MODE_0_OUTL, MODE_0_OUTR, MODE_0_COMB_FACTOR, MODE_0_COMB_FEEDBACK, MODE_0_DRY_AMP, MODE_0_WET_AMP, MODE_0_LPF_AMP };
	static const BReverbSettings REVERB_MODE_1_SETTINGS = { MODE_1_NUMBER_OF_ALLPASSES, MODE_1_ALLPASSES, MODE_1_NUMBER_OF_COMBS, MODE_1_COMBS, MODE_1_OUTL, MODE_1_OUTR, MODE_1_COMB_FACTOR, MODE_1_COMB_FEEDBACK, MODE_1_DRY_AMP, MODE_1_WET_AMP, MODE_1_LPF_AMP };
	static const BReverbSettings REVERB_MODE_2_SETTINGS = { MODE_2_NUMBER_OF_ALLPASSES, MODE_2_ALLPASSES, MODE_2_NUMBER_OF_COMBS, MODE_2_COMBS, MODE_2_OUTL, MODE_2_OUTR, MODE_2_COMB_FACTOR, MODE_2_COMB_FEEDBACK, MODE_2_DRY_AMP, MODE_2_WET_AMP, MODE_2_LPF_AMP };
	static const BReverbSettings REVERB_MODE_3_SETTINGS = { MODE_3_NUMBER_OF_ALLPASSES, NULL, MODE_3_NUMBER_OF_COMBS, MODE_3_DELAY, MODE_3_OUTL, MODE_3_OUTR, MODE_3_COMB_FACTOR, MODE_3_COMB_FEEDBACK, MODE_3_DRY_AMP, MODE_3_WET_AMP, 0 };

	static const BReverbSettings * const REVERB_SETTINGS[] = { &REVERB_MODE_0_SETTINGS, &REVERB_MODE_1_SETTINGS, &REVERB_MODE_2_SETTINGS, &REVERB_MODE_3_SETTINGS };

	return *REVERB_SETTINGS[mode];
}

// Default reverb settings for "old" reverb model implemented in MT-32.
// Found by tracing reverb RAM data lines (thanks go to Lord_Nightmare & balrog).
static const BReverbSettings &getMT32Settings(const ReverbMode mode) {
	static const u32 MODE_0_NUMBER_OF_ALLPASSES = 3;
	static const u32 MODE_0_ALLPASSES[] = { 994, 729, 78 };
	static const u32 MODE_0_NUMBER_OF_COMBS = 4; // Same as above in the new model implementation
	static const u32 MODE_0_COMBS[] = { 575 + PROCESS_DELAY, 2040, 2752, 3629 };
	static const u32 MODE_0_OUTL[] = { 2040, 687, 1814 };
	static const u32 MODE_0_OUTR[] = { 1019, 2072, 1 };
	static const u8  MODE_0_COMB_FACTOR[] = { 0xB0, 0x60, 0x60, 0x60 };
	static const u8  MODE_0_COMB_FEEDBACK[] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
												  0x28, 0x48, 0x60, 0x70, 0x78, 0x80, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98 };
	static const u8  MODE_0_DRY_AMP[] = { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 };
	static const u8  MODE_0_WET_AMP[] = { 0x10, 0x20, 0x30, 0x40, 0x50, 0x70, 0xA0, 0xE0 };
	static const u8  MODE_0_LPF_AMP = 0x80;

	static const u32 MODE_1_NUMBER_OF_ALLPASSES = 3;
	static const u32 MODE_1_ALLPASSES[] = { 1324, 809, 176 };
	static const u32 MODE_1_NUMBER_OF_COMBS = 4; // Same as above in the new model implementation
	static const u32 MODE_1_COMBS[] = { 961 + PROCESS_DELAY, 2619, 3545, 4519 };
	static const u32 MODE_1_OUTL[] = { 2618, 1760, 4518 };
	static const u32 MODE_1_OUTR[] = { 1300, 3532, 2274 };
	static const u8  MODE_1_COMB_FACTOR[] = { 0x90, 0x60, 0x60, 0x60 };
	static const u8  MODE_1_COMB_FEEDBACK[] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
												  0x28, 0x48, 0x60, 0x70, 0x78, 0x80, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98 };
	static const u8  MODE_1_DRY_AMP[] = { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 };
	static const u8  MODE_1_WET_AMP[] = { 0x10, 0x20, 0x30, 0x40, 0x50, 0x70, 0xA0, 0xE0 };
	static const u8  MODE_1_LPF_AMP = 0x80;

	static const u32 MODE_2_NUMBER_OF_ALLPASSES = 3;
	static const u32 MODE_2_ALLPASSES[] = { 969, 644, 157 };
	static const u32 MODE_2_NUMBER_OF_COMBS = 4; // Same as above in the new model implementation
	static const u32 MODE_2_COMBS[] = { 116 + PROCESS_DELAY, 2259, 2839, 3539 };
	static const u32 MODE_2_OUTL[] = { 2259, 718, 1769 };
	static const u32 MODE_2_OUTR[] = { 1136, 2128, 1 };
	static const u8  MODE_2_COMB_FACTOR[] = { 0, 0x60, 0x60, 0x60 };
	static const u8  MODE_2_COMB_FEEDBACK[] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
												  0x28, 0x48, 0x60, 0x70, 0x78, 0x80, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98,
												  0x28, 0x48, 0x60, 0x78, 0x80, 0x88, 0x90, 0x98 };
	static const u8  MODE_2_DRY_AMP[] = { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 };
	static const u8  MODE_2_WET_AMP[] = { 0x10, 0x20, 0x30, 0x40, 0x50, 0x70, 0xA0, 0xE0 };
	static const u8  MODE_2_LPF_AMP = 0x80;

	static const u32 MODE_3_NUMBER_OF_ALLPASSES = 0;
	static const u32 MODE_3_NUMBER_OF_COMBS = 1;
	static const u32 MODE_3_DELAY[] = { 16000 + MODE_3_FEEDBACK_DELAY + PROCESS_DELAY + MODE_3_ADDITIONAL_DELAY };
	static const u32 MODE_3_OUTL[] = { 400, 624, 960, 1488, 2256, 3472, 5280, 8000 };
	static const u32 MODE_3_OUTR[] = { 800, 1248, 1920, 2976, 4512, 6944, 10560, 16000 };
	static const u8  MODE_3_COMB_FACTOR[] = { 0x68 };
	static const u8  MODE_3_COMB_FEEDBACK[] = { 0x68, 0x60 };
	static const u8  MODE_3_DRY_AMP[] = { 0x10, 0x10, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
											0x10, 0x20, 0x20, 0x10, 0x20, 0x10, 0x20, 0x10 };
	static const u8  MODE_3_WET_AMP[] = { 0x08, 0x18, 0x28, 0x40, 0x60, 0x80, 0xA8, 0xF8 };

	static const BReverbSettings REVERB_MODE_0_SETTINGS = { MODE_0_NUMBER_OF_ALLPASSES, MODE_0_ALLPASSES, MODE_0_NUMBER_OF_COMBS, MODE_0_COMBS, MODE_0_OUTL, MODE_0_OUTR, MODE_0_COMB_FACTOR, MODE_0_COMB_FEEDBACK, MODE_0_DRY_AMP, MODE_0_WET_AMP, MODE_0_LPF_AMP };
	static const BReverbSettings REVERB_MODE_1_SETTINGS = { MODE_1_NUMBER_OF_ALLPASSES, MODE_1_ALLPASSES, MODE_1_NUMBER_OF_COMBS, MODE_1_COMBS, MODE_1_OUTL, MODE_1_OUTR, MODE_1_COMB_FACTOR, MODE_1_COMB_FEEDBACK, MODE_1_DRY_AMP, MODE_1_WET_AMP, MODE_1_LPF_AMP };
	static const BReverbSettings REVERB_MODE_2_SETTINGS = { MODE_2_NUMBER_OF_ALLPASSES, MODE_2_ALLPASSES, MODE_2_NUMBER_OF_COMBS, MODE_2_COMBS, MODE_2_OUTL, MODE_2_OUTR, MODE_2_COMB_FACTOR, MODE_2_COMB_FEEDBACK, MODE_2_DRY_AMP, MODE_2_WET_AMP, MODE_2_LPF_AMP };
	static const BReverbSettings REVERB_MODE_3_SETTINGS = { MODE_3_NUMBER_OF_ALLPASSES, NULL, MODE_3_NUMBER_OF_COMBS, MODE_3_DELAY, MODE_3_OUTL, MODE_3_OUTR, MODE_3_COMB_FACTOR, MODE_3_COMB_FEEDBACK, MODE_3_DRY_AMP, MODE_3_WET_AMP, 0 };

	static const BReverbSettings * const REVERB_SETTINGS[] = { &REVERB_MODE_0_SETTINGS, &REVERB_MODE_1_SETTINGS, &REVERB_MODE_2_SETTINGS, &REVERB_MODE_3_SETTINGS };

	return *REVERB_SETTINGS[mode];
}

static inline s16 weirdMul(s16 sample, u8 addMask, u8 carryMask) {
#if MT32EMU_BOSS_REVERB_PRECISE_MODE
	// This algorithm tries to emulate exactly Boss multiplication operation (at least this is what we see on reverb RAM data lines).
	u8 mask = 0x80;
	s32 res = 0;
	for (int i = 0; i < 8; i++) {
		s32 carry = (sample < 0) && (mask & carryMask) > 0 ? sample & 1 : 0;
		sample >>= 1;
		res += (mask & addMask) > 0 ? sample + carry : 0;
		mask >>= 1;
	}
	return s16(res);
#else
	(void)carryMask;
	return s16((s32(sample) * addMask) >> 8);
#endif
}

static inline float weirdMul(float sample, u8 addMask, u8 carryMask) {
	(void)carryMask;
	return sample * addMask / 256.0f;
}

static inline s16 halveSample(s16 sample) {
	return sample >> 1;
}

static inline float halveSample(float sample) {
	return 0.5f * sample;
}

static inline s16 quarterSample(s16 sample) {
#if MT32EMU_BOSS_REVERB_PRECISE_MODE
	return (sample >> 1) / 2;
#else
	return sample >> 2;
#endif
}

static inline float quarterSample(float sample) {
	return 0.25f * sample;
}

static inline s16 addDCBias(s16 sample) {
	return sample;
}

static inline float addDCBias(float sample) {
	return sample + BIAS;
}

static inline s16 addAllpassNoise(s16 sample) {
#if MT32EMU_BOSS_REVERB_PRECISE_MODE
	// This introduces reverb noise which actually makes output from the real Boss chip nondeterministic
	return sample - 1;
#else
	return sample;
#endif
}

static inline float addAllpassNoise(float sample) {
	return sample;
}

static inline s16 clipSampleEx(s32 sampleEx) {
	// Clamp values above 32767 to 32767, and values below -32768 to -32768
	// FIXME: Do we really need this stuff? I think these branches are very well predicted. Instead, this introduces a chain.
	// The version below is actually a bit faster on my system...
	//return ((sampleEx + 0x8000) & ~0xFFFF) ? Bit16s((sampleEx >> 31) ^ 0x7FFF) : (Bit16s)sampleEx;
	return ((-0x8000 <= sampleEx) && (sampleEx <= 0x7FFF)) ? s16(sampleEx) : s16((sampleEx >> 31) ^ 0x7FFF);
}

/* NOTE:
 *   Thanks to Mok for discovering, the adder in BOSS reverb chip is found to perform addition with saturation to avoid integer overflow.
 *   Analysing of the algorithm suggests that the overflow is most probable when the combs output is added below.
 *   So, despite this isn't actually accurate, we only add the check here for performance reasons.
 */
static inline s16 mixCombs(s16 out1, s16 out2, s16 out3) {
#if MT32EMU_BOSS_REVERB_PRECISE_MODE
	return clipSampleEx(clipSampleEx(clipSampleEx(clipSampleEx(s32(out1) + (s32(out1) >> 1)) + s32(out2)) + (s32(out2) >> 1)) + s32(out3));
#else
	return clipSampleEx(s32(out1) + (s32(out1) >> 1) + s32(out2) + (s32(out2) >> 1) + s32(out3));
#endif
}

static inline float mixCombs(float out1, float out2, float out3) {
	return 1.5f * (out1 + out2) + out3;
}

static inline void muteSampleBuffer(float *buffer, u32 len) {
	if (buffer == NULL) return;
	// FIXME: Use memset() where compatibility is guaranteed (if this turns out to be a win)
	while (len--) {
		*(buffer++) = 0.0f;
	}
}

static inline void muteSampleBuffer(s16 *buffer, u32 len) {
	if (buffer == NULL) return;
	// FIXME: Use memset() where compatibility is guaranteed (if this turns out to be a win)
	while (len--) {
		*(buffer++) = 0;
	}
}

template <class Sample>
class RingBuffer {
	static inline Sample sampleValueThreshold();

protected:
	Sample * buffer;
	const u32 size;
	u32 index;

public:
	RingBuffer(const u32 newsize) : size(newsize), index(0) {
		buffer = new Sample[size];
	}

	virtual ~RingBuffer() {
		delete[] buffer;
		buffer = NULL;
	}

	Sample next() {
		if (++index >= size) {
			index = 0;
		}
		return buffer[index];
	}

	bool isEmpty() const {
		if (buffer == NULL) return true;

		Sample *buf = buffer;
		for (u32 i = 0; i < size; i++) {
			if (*buf < -sampleValueThreshold() || *buf > sampleValueThreshold()) return false;
			buf++;
		}
		return true;
	}

	void mute() {
		muteSampleBuffer(buffer, size);
	}
};

template<>
s16 RingBuffer<s16>::sampleValueThreshold() {
	return 8;
}

template<>
float RingBuffer<float>::sampleValueThreshold() {
	return 0.001f;
}

template <class Sample>
class AllpassFilter : public RingBuffer<Sample> {
public:
	AllpassFilter(const u32 useSize) : RingBuffer<Sample>(useSize) {}

	// This model corresponds to the allpass filter implementation of the real CM-32L device
	// found from sample analysis
	Sample process(const Sample in) {
		const Sample bufferOut = this->next();

		// store input - feedback / 2
		this->buffer[this->index] = in - halveSample(bufferOut);

		// return buffer output + feedforward / 2
		return bufferOut + halveSample(this->buffer[this->index]);
	}
};

template <class Sample>
class CombFilter : public RingBuffer<Sample> {
protected:
	const u8 filterFactor;
	u8 feedbackFactor;

public:
	CombFilter(const u32 useSize, const u8 useFilterFactor) : RingBuffer<Sample>(useSize), filterFactor(useFilterFactor) {}

	// This model corresponds to the comb filter implementation of the real CM-32L device
	void process(const Sample in) {

		// the previously stored value
		const Sample last = this->buffer[this->index];

		// prepare input + feedback
		const Sample filterIn = in + weirdMul(this->next(), feedbackFactor, 0xF0);

		// store input + feedback processed by a low-pass filter
		this->buffer[this->index] = weirdMul(last, filterFactor, 0xC0) - filterIn;
	}

	Sample getOutputAt(const u32 outIndex) const {
		return this->buffer[(this->size + this->index - outIndex) % this->size];
	}

	void setFeedbackFactor(const u8 useFeedbackFactor) {
		feedbackFactor = useFeedbackFactor;
	}
};

template <class Sample>
class DelayWithLowPassFilter : public CombFilter<Sample> {
	u8 amp;

public:
	DelayWithLowPassFilter(const u32 useSize, const u8 useFilterFactor, const u8 useAmp)
		: CombFilter<Sample>(useSize, useFilterFactor), amp(useAmp) {}

	void process(const Sample in) {
		// the previously stored value
		const Sample last = this->buffer[this->index];

		// move to the next index
		this->next();

		// low-pass filter process
		Sample lpfOut = weirdMul(last, this->filterFactor, 0xFF) + in;

		// store lpfOut multiplied by LPF amp factor
		this->buffer[this->index] = weirdMul(lpfOut, amp, 0xFF);
	}
};

template <class Sample>
class TapDelayCombFilter : public CombFilter<Sample> {
	u32 outL;
	u32 outR;

public:
	TapDelayCombFilter(const u32 useSize, const u8 useFilterFactor) : CombFilter<Sample>(useSize, useFilterFactor) {}

	void process(const Sample in) {
		// the previously stored value
		const Sample last = this->buffer[this->index];

		// move to the next index
		this->next();

		// prepare input + feedback
		// Actually, the size of the filter varies with the TIME parameter, the feedback sample is taken from the position just below the right output
		const Sample filterIn = in + weirdMul(this->getOutputAt(outR + MODE_3_FEEDBACK_DELAY), this->feedbackFactor, 0xF0);

		// store input + feedback processed by a low-pass filter
		this->buffer[this->index] = weirdMul(last, this->filterFactor, 0xF0) - filterIn;
	}

	Sample getLeftOutput() const {
		return this->getOutputAt(outL + PROCESS_DELAY + MODE_3_ADDITIONAL_DELAY);
	}

	Sample getRightOutput() const {
		return this->getOutputAt(outR + PROCESS_DELAY + MODE_3_ADDITIONAL_DELAY);
	}

	void setOutputPositions(const u32 useOutL, const u32 useOutR) {
		outL = useOutL;
		outR = useOutR;
	}
};

template <class Sample>
class BReverbModelImpl : public CM32P_BReverbModel {
public:
	AllpassFilter<Sample> **allpasses;
	CombFilter<Sample> **combs;

	const BReverbSettings &currentSettings;
	const bool tapDelayMode;
	u8 dryAmp;
	u8 wetLevel;

	BReverbModelImpl(const ReverbMode mode, const bool mt32CompatibleModel) :
		allpasses(NULL), combs(NULL),
		currentSettings(mt32CompatibleModel ? getMT32Settings(mode) : getCM32L_LAPCSettings(mode)),
		tapDelayMode(mode == REVERB_MODE_TAP_DELAY)
	{}

	~BReverbModelImpl() {
		close();
	}

	bool isOpen() const {
		return combs != NULL;
	}

	void open() {
		if (isOpen()) return;
		if (currentSettings.numberOfAllpasses > 0) {
			allpasses = new AllpassFilter<Sample>*[currentSettings.numberOfAllpasses];
			for (u32 i = 0; i < currentSettings.numberOfAllpasses; i++) {
				allpasses[i] = new AllpassFilter<Sample>(currentSettings.allpassSizes[i]);
			}
		}
		combs = new CombFilter<Sample>*[currentSettings.numberOfCombs];
		if (tapDelayMode) {
			*combs = new TapDelayCombFilter<Sample>(*currentSettings.combSizes, *currentSettings.filterFactors);
		}
		else {
			combs[0] = new DelayWithLowPassFilter<Sample>(currentSettings.combSizes[0], currentSettings.filterFactors[0], currentSettings.lpfAmp);
			for (u32 i = 1; i < currentSettings.numberOfCombs; i++) {
				combs[i] = new CombFilter<Sample>(currentSettings.combSizes[i], currentSettings.filterFactors[i]);
			}
		}
		mute();
	}

	void close() {
		if (allpasses != NULL) {
			for (u32 i = 0; i < currentSettings.numberOfAllpasses; i++) {
				if (allpasses[i] != NULL) {
					delete allpasses[i];
					allpasses[i] = NULL;
				}
			}
			delete[] allpasses;
			allpasses = NULL;
		}
		if (combs != NULL) {
			for (u32 i = 0; i < currentSettings.numberOfCombs; i++) {
				if (combs[i] != NULL) {
					delete combs[i];
					combs[i] = NULL;
				}
			}
			delete[] combs;
			combs = NULL;
		}
	}

	void mute() {
		if (allpasses != NULL) {
			for (u32 i = 0; i < currentSettings.numberOfAllpasses; i++) {
				allpasses[i]->mute();
			}
		}
		if (combs != NULL) {
			for (u32 i = 0; i < currentSettings.numberOfCombs; i++) {
				combs[i]->mute();
			}
		}
	}

	void setParameters(u8 time, u8 level) {
		if (!isOpen()) return;
		level &= 7;
		time &= 7;
		if (tapDelayMode) {
			TapDelayCombFilter<Sample> *comb = static_cast<TapDelayCombFilter<Sample> *> (*combs);
			comb->setOutputPositions(currentSettings.outLPositions[time], currentSettings.outRPositions[time & 7]);
			comb->setFeedbackFactor(currentSettings.feedbackFactors[((level < 3) || (time < 6)) ? 0 : 1]);
		}
		else {
			for (u32 i = 1; i < currentSettings.numberOfCombs; i++) {
				combs[i]->setFeedbackFactor(currentSettings.feedbackFactors[(i << 3) + time]);
			}
		}
		if (time == 0 && level == 0) {
			dryAmp = wetLevel = 0;
		}
		else {
			if (tapDelayMode && ((time == 0) || (time == 1 && level == 1))) {
				// Looks like MT-32 implementation has some minor quirks in this mode:
				// for odd level values, the output level changes sometimes depending on the time value which doesn't seem right.
				dryAmp = currentSettings.dryAmps[level + 8];
			}
			else {
				dryAmp = currentSettings.dryAmps[level];
			}
			wetLevel = currentSettings.wetLevels[level];
		}
	}

	bool isActive() const {
		if (!isOpen()) return false;
		for (u32 i = 0; i < currentSettings.numberOfAllpasses; i++) {
			if (!allpasses[i]->isEmpty()) return true;
		}
		for (u32 i = 0; i < currentSettings.numberOfCombs; i++) {
			if (!combs[i]->isEmpty()) return true;
		}
		return false;
	}

	bool isMT32Compatible(ReverbMode mode) const {
		return &currentSettings == &getMT32Settings(mode);
	}

	template <class SampleEx>
	void produceOutput(const Sample *inLeft, const Sample *inRight, Sample *outLeft, Sample *outRight, u32 numSamples) {
		if (!isOpen()) {
			muteSampleBuffer(outLeft, numSamples);
			muteSampleBuffer(outRight, numSamples);
			return;
		}

		while ((numSamples--) > 0) {
			Sample dry;

			if (tapDelayMode) {
				dry = halveSample(*(inLeft++)) + halveSample(*(inRight++));
			}
			else {
				dry = quarterSample(*(inLeft++)) + quarterSample(*(inRight++));
			}

			// Looks like dryAmp doesn't change in MT-32 but it does in CM-32L / LAPC-I
			dry = weirdMul(addDCBias(dry), dryAmp, 0xFF);

			if (tapDelayMode) {
				TapDelayCombFilter<Sample> *comb = static_cast<TapDelayCombFilter<Sample> *>(*combs);
				comb->process(dry);
				if (outLeft != NULL) {
					*(outLeft++) = weirdMul(comb->getLeftOutput(), wetLevel, 0xFF);
				}
				if (outRight != NULL) {
					*(outRight++) = weirdMul(comb->getRightOutput(), wetLevel, 0xFF);
				}
			}
			else {
				DelayWithLowPassFilter<Sample> * const entranceDelay = static_cast<DelayWithLowPassFilter<Sample> *>(combs[0]);
				// If the output position is equal to the comb size, get it now in order not to loose it
				Sample link = entranceDelay->getOutputAt(currentSettings.combSizes[0] - 1);

				// Entrance LPF. Note, comb.process() differs a bit here.
				entranceDelay->process(dry);

				link = allpasses[0]->process(addAllpassNoise(link));
				link = allpasses[1]->process(link);
				link = allpasses[2]->process(link);

				// If the output position is equal to the comb size, get it now in order not to loose it
				Sample outL1 = combs[1]->getOutputAt(currentSettings.outLPositions[0] - 1);

				combs[1]->process(link);
				combs[2]->process(link);
				combs[3]->process(link);

				if (outLeft != NULL) {
					Sample outL2 = combs[2]->getOutputAt(currentSettings.outLPositions[1]);
					Sample outL3 = combs[3]->getOutputAt(currentSettings.outLPositions[2]);
					Sample outSample = mixCombs(outL1, outL2, outL3);
					*(outLeft++) = weirdMul(outSample, wetLevel, 0xFF);
				}
				if (outRight != NULL) {
					Sample outR1 = combs[1]->getOutputAt(currentSettings.outRPositions[0]);
					Sample outR2 = combs[2]->getOutputAt(currentSettings.outRPositions[1]);
					Sample outR3 = combs[3]->getOutputAt(currentSettings.outRPositions[2]);
					Sample outSample = mixCombs(outR1, outR2, outR3);
					*(outRight++) = weirdMul(outSample, wetLevel, 0xFF);
				}
			} // if (tapDelayMode)
		} // while ((numSamples--) > 0)
	} // produceOutput

	bool process(const s16 *inLeft, const s16 *inRight, s16 *outLeft, s16 *outRight, u32 numSamples);
	bool process(const float *inLeft, const float *inRight, float *outLeft, float *outRight, u32 numSamples);
};

CM32P_BReverbModel *CM32P_BReverbModel::createBReverbModel(ReverbMode mode, bool mt32CompatibleModel, RendererType rendererType) {
	switch (rendererType)
	{
	case RendererType::REVERB_BIT16S:
		return new BReverbModelImpl<s16>(mode, mt32CompatibleModel);
	case RendererType::REVERB_FLOAT:
		return new BReverbModelImpl<float>(mode, mt32CompatibleModel);
	}
	return NULL;
}

template <>
bool BReverbModelImpl<s16>::process(const s16 *inLeft, const s16 *inRight, s16 *outLeft, s16 *outRight, u32 numSamples) {
	produceOutput<s32>(inLeft, inRight, outLeft, outRight, numSamples);
	return true;
}

template <>
bool BReverbModelImpl<s16>::process(const float *, const float *, float *, float *, u32) {
	return false;
}

template <>
bool BReverbModelImpl<float>::process(const s16 *, const s16 *, s16 *, s16 *, u32) {
	return false;
}

template <>
bool BReverbModelImpl<float>::process(const float *inLeft, const float *inRight, float *outLeft, float *outRight, u32 numSamples) {
	produceOutput<float>(inLeft, inRight, outLeft, outRight, numSamples);
	return true;
}

