#include "emu.h"
#include "emumem.h"
#include "machine.h"
#include "..\frontend\mame\mame.h"
#include "..\frontend\mame\cheat.h"
#include "..\devices\sound\fm.h"
#include "..\devices\sound\ym2151.h"
#include "..\devices\sound\2612intf.h"
#include "..\devices\sound\gb.h"
#include "..\devices\sound\sn76496.h"
#include "..\devices\sound\namco.h"

#define DllExport extern "C" __declspec (dllexport)

address_space *dummy;

extern "C"
{
	//memodimemo

	DllExport void ym2151_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr)
			return;

		std::string num = std::to_string(unitNumber);
		ym2151_device *ym2151 = dynamic_cast<ym2151_device *>(rm->device((std::string("ym2151_") + num).c_str()));
		if (ym2151 == nullptr)
			return;

		ym2151->write(address, data);
	}

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

	DllExport void gb_apu_wave_write(unsigned int unitNumber, unsigned int address, unsigned char data)
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

		gb_apu->wave_w(address, data);
	}

	DllExport void sn76496_write(unsigned int unitNumber, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr)
			return;

		std::string num = std::to_string(unitNumber);
		sn76496_base_device *sn76496 = dynamic_cast<sn76496_base_device *>(rm->device((std::string("sn76496_") + num).c_str()));
		if (sn76496 == nullptr)
			return;

		sn76496->write(data);
	}

	DllExport void namco_cus30_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr)
			return;

		std::string num = std::to_string(unitNumber);
		namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->device((std::string("namco_cus30_") + num).c_str()));
		if (cus30 == nullptr)
			return;

		cus30->namcos1_cus30_w(*dummy, address, data);
	}

	DllExport unsigned char namco_cus30_r(unsigned int unitNumber, unsigned int address)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return 0;
		running_machine *rm = mmm->machine();
		if (rm == nullptr)
			return 0;

		std::string num = std::to_string(unitNumber);
		namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->device((std::string("namco_cus30_") + num).c_str()));
		if (cus30 == nullptr)
			return 0;

		return cus30->namcos1_cus30_r(*dummy, address);
	}
}


