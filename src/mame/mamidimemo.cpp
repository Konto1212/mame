#include "emu.h"
#include "machine.h"
#include "..\frontend\mame\mame.h"
#include "..\frontend\mame\cheat.h"
#include "..\devices\sound\fm.h"
#include "..\devices\sound\2612intf.h"
#include "..\devices\sound\gb.h"

#define DllExport extern "C" __declspec (dllexport)

extern "C"
{

	DllExport void ym2612_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		//std::unique_ptr<debugger_cpu> &dcpu = mame_machine_manager::instance()->cheat().cpu();
		//address_space &mem = dcpu->get_visible_cpu()->memory().space(AS_PROGRAM);
		//dcpu->write_byte(mem, address, data, false);

		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr)
			return;

		std::string num = std::to_string(unitNumber);
		ym2612_device *ym2612 = dynamic_cast<ym2612_device *>(rm->device((std::string("ym2612_") + num).c_str()));
		if (ym2612 == nullptr)
			return;

		ym2612->write(address, data);
	}

	DllExport void gb_apu_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr)
			return;

		std::string num = std::to_string(unitNumber);
		dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
		if (gb_apu == nullptr)
			return;

		gb_apu->sound_w(address, data);
	}

	DllExport unsigned char gb_apu_read(unsigned int unitNumber, unsigned int address)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return 0;
		running_machine *rm = mmm->machine();
		if (rm == nullptr)
			return 0;

		std::string num = std::to_string(unitNumber);
		dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
		if (gb_apu == nullptr)
			return 0;

		return gb_apu->sound_r(address);
	}
}


