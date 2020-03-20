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

#include "emu.h"

#ifndef CM32PEMU_B_REVERB_MODEL_H
#define CM32PEMU_B_REVERB_MODEL_H

#define CM32PEMU_BOSS_REVERB_PRECISE_MODE 1

#include "cm32p.h"

enum RendererType {
	/** Use 16-bit signed samples in the renderer and the accurate wave generator model based on logarithmic fixed-point computations and LUTs. Maximum emulation accuracy and speed. */
	REVERB_BIT16S,
	/** Use float samples in the renderer and simplified wave generator model. Maximum output quality and minimum noise. */
	REVERB_FLOAT
};

class CM32P_BReverbModel {
public:
	static CM32P_BReverbModel *createBReverbModel(ReverbMode mode, bool mt32CompatibleModel, RendererType rendererType);

	virtual ~CM32P_BReverbModel() {}
	virtual bool isOpen() const = 0;
	// After construction or a close(), open() must be called at least once before any other call (with the exception of close()).
	virtual void open() = 0;
	// May be called multiple times without an open() in between.
	virtual void close() = 0;
	virtual void mute() = 0;
	virtual void setParameters(u8 time, u8 level) = 0;
	virtual bool isActive() const = 0;
	virtual bool isMT32Compatible(ReverbMode mode) const = 0;
	virtual bool process(const s16 *inLeft, const s16 *inRight, s16 *outLeft, s16 *outRight, u32 numSamples) = 0;
};

#endif // #ifndef CM32PEMU_B_REVERB_MODEL_H
