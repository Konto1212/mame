#include "stdafx.h"

#include "windows.h"
#include "wrapper.h"
//#include "..\frontend\mame\mame.h"
#include "..\mame\mamidimemo.h"

using namespace System;
using namespace zanac::mamidimemo;

extern "C" {

	// 明示的ロードでVC、DOTNETから使用するのでヘッダーファイルは不要
	// なお、__cdeclでもDOTNETから正しく呼び出せることを確認している。
	// 通常、__stdcallを適用する(__stdcall = WINAPI)。

	__declspec(dllexport) void __stdcall MainWarpper(HMODULE hParentModule)
	{
		zanac::mamidimemo::Program::Main((IntPtr)hParentModule);
	}

	__declspec(dllexport) int __stdcall HasExited()
	{
		return zanac::mamidimemo::Program::HasExited();
	}
}

