// copyright-holders:K.Ito
#include <windows.h>
#include <stdio.h>

//memidimemo

typedef void(CALLBACK* MainWrapperProc)(HMODULE);

typedef int(CALLBACK* HasExitedProc)();
typedef void(CALLBACK* SoundUpdatingProc)();
typedef void(CALLBACK* SoundUpdatedProc)();
typedef void(CALLBACK* RestartApplicationProc)();


HasExitedProc hasExited;
SoundUpdatingProc soundUpdating;
SoundUpdatedProc soundUpdated;
RestartApplicationProc restartApplication;

DWORD WINAPI StartMAmidiMEmoMainThread(LPVOID lpParam)
{
	HMODULE hModule = LoadLibrary("wrapper.dll");
	FARPROC proc = GetProcAddress(hModule, "_MainWarpper@4");
	if (proc != NULL)
	{
		MainWrapperProc main = reinterpret_cast<MainWrapperProc>(proc);
		main(GetModuleHandle(NULL));
	}
	return 0;
}

void StartMAmidiMEmoMain()
{
	HMODULE hModule = LoadLibrary("wrapper.dll");
	FARPROC proc;

	proc = GetProcAddress(hModule, "_HasExited@0");
	if (proc != NULL)
		hasExited = reinterpret_cast<HasExitedProc>(proc);
	proc = GetProcAddress(hModule, "_SoundUpdating@0");
	if (proc != NULL)
		soundUpdating = reinterpret_cast<SoundUpdatingProc>(proc);
	proc = GetProcAddress(hModule, "_SoundUpdated@0");
	if (proc != NULL)
		soundUpdated = reinterpret_cast<SoundUpdatedProc>(proc);
	proc = GetProcAddress(hModule, "_RestartApplication@0");
	if (proc != NULL)
		restartApplication = reinterpret_cast<RestartApplicationProc>(proc);

	proc = GetProcAddress(hModule, "_MainWarpper@4");
	if (proc != NULL)
	{
		MainWrapperProc main = reinterpret_cast<MainWrapperProc>(proc);
		main(GetModuleHandle(NULL));
	}
	/*
	DWORD dwThreadId = 0L;
	HANDLE hThread = CreateThread(NULL, 0, StartMAmidiMEmoMainThread, 0, 0, &dwThreadId);
	WaitForSingleObject(hThread, INFINITE);
	CloseHandle(hThread);*/
}

int HasExited()
{
	return hasExited();
}

void SoundUpdating()
{
	return soundUpdating();
}

void SoundUpdated()
{
	return soundUpdated();
}

void MamiOutputDebugString(char* pszFormat, ...)
{
	va_list	argp;
	char pszBuf[256];
	va_start(argp, pszFormat);
	vsprintf(pszBuf, pszFormat, argp);
	va_end(argp);
	OutputDebugString(pszBuf);
}

void RestartApplication()
{
	return restartApplication();
}