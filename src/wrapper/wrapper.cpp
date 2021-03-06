// copyright-holders:K.Ito
#include "stdafx.h"

#include "windows.h"
#include "wrapper.h"
//#include "..\frontend\mame\mame.h"
#include "..\mame\MAmidiMEmo.h"
#include <vcclr.h>

using namespace System;
using namespace zanac::MAmidiMEmo;

extern "C" {

	//MAmidiMEmo

	// 明示的ロードでVC、DOTNETから使用するのでヘッダーファイルは不要
	// なお、__cdeclでもDOTNETから正しく呼び出せることを確認している。
	// 通常、__stdcallを適用する(__stdcall = WINAPI)。

	__declspec(dllexport) void __stdcall MainWarpper(HMODULE hParentModule)
	{
		zanac::MAmidiMEmo::Program::Main((IntPtr)hParentModule);
	}

	__declspec(dllexport) int __stdcall HasExited()
	{
		return zanac::MAmidiMEmo::Program::HasExited();
	}

	__declspec(dllexport) void __stdcall SoundUpdating()
	{
		zanac::MAmidiMEmo::Program::SoundUpdating();
	}

	__declspec(dllexport) void __stdcall SoundUpdated()
	{
		zanac::MAmidiMEmo::Program::SoundUpdated();
	}

	__declspec(dllexport) void __stdcall RestartApplication()
	{
		zanac::MAmidiMEmo::Program::RestartApplication();
	}
}

