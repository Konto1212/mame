// copyright-holders:K.Ito
#include "emu.h"
#include "emumem.h"
#include "machine.h"
#include "..\frontend\mame\mame.h"
#include "..\frontend\mame\cheat.h"
#include "..\devices\sound\fm.h"
#include "..\devices\sound\ym2151.h"
#include "..\devices\sound\ym2413.h"
#include "..\devices\sound\2612intf.h"
#include "..\devices\sound\gb.h"
#include "..\devices\sound\sn76496.h"
#include "..\devices\sound\namco.h"
#include "..\devices\sound\nes_apu.h"
#include "..\devices\sound\3812intf.h"
#include "..\devices\sound\k051649.h"
#include "..\devices\sound\msm5232.h"
#include "..\devices\sound\ay8910.h"
#include "..\devices\sound\mos6581.h"
#include "..\devices\sound\beep.h"
#include "..\devices\sound\c140.h"

#define DllExport extern "C" __declspec (dllexport)

address_space *dummy;

extern "C"
{
	//memodimemo


	DllExport void device_reset(unsigned int unitNumber, char* name)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);

		device_t *dev = dynamic_cast<device_t  *>(rm->device((std::string(name) + num).c_str()));
		//device_t *dev = dynamic_cast<device_t  *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (dev == nullptr)
			return;

		dev->reset();
	}

	DllExport void set_device_enable(unsigned int unitNumber, char* name, int enable)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->m_enable = enable;
	}


	DllExport void set_clock(unsigned int unitNumber, char* name, unsigned int clock)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_t *dev = dynamic_cast<device_t  *>(rm->device((std::string(name) + num).c_str()));
		//device_t *dev = dynamic_cast<device_t  *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (dev == nullptr)
			return;

		dev->set_clock(clock);
	}

	DllExport void set_filter(int unitNumber, char* name, device_sound_interface::FilterMode filterMode, double cutoff, double resonance)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->setFilterMode(filterMode);
		sd->setCutoff(cutoff);
		sd->setResonance(resonance);
	}

	DllExport void set_output_gain(unsigned int unitNumber, char* name, int channel, float gain)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->set_output_gain(channel, gain);
	}

	DllExport s32 *getLastOutputBuffer(char* name, int insts, unsigned int deviceId, unsigned int unitNumber)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return NULL;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return NULL;

		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name)).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name)).c_str()));
		if (sd == nullptr)
			return NULL;

		//limitation: 1st buffer is last unit buffer data
		if (deviceId == UINT32_MAX)
			sd->lastOutBufferNumber = UINT32_MAX;
		else
			sd->lastOutBufferNumber = (insts * unitNumber) + (deviceId - 1);

		return sd->lastOutBuffer;
	}

	DllExport int getLastOutputBufferSamples(char* name)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return 0;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return 0;

		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name)).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name)).c_str()));
		if (sd == nullptr)
			return 0;
		return sd->lastOutBufferSamples;
	}


	DllExport void ym2151_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		ym2151_device *ym2151 = dynamic_cast<ym2151_device *>(rm->device((std::string("ym2151_") + num).c_str()));
		//ym2151_device *ym2151 = dynamic_cast<ym2151_device *>(rm->root_device().subdevice((std::string("ym2151_") + num).c_str()));
		if (ym2151 == nullptr)
			return;

		ym2151->write(address, data);
	}

	DllExport void ym2612_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{

		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		ym2612_device *ym2612 = dynamic_cast<ym2612_device *>(rm->device((std::string("ym2612_") + num).c_str()));
		//ym2612_device *ym2612 = dynamic_cast<ym2612_device *>(rm->root_device().subdevice((std::string("ym2612_") + num).c_str()));
		if (ym2612 == nullptr)
			return;

		ym2612->write(address, data);
	}

	DllExport void ym3812_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		ym3812_device *ym3812 = dynamic_cast<ym3812_device *>(rm->device((std::string("ym3812_") + num).c_str()));
		//ym3812_device *ym3812 = dynamic_cast<ym3812_device *>(rm->root_device().subdevice((std::string("ym3812_") + num).c_str()));
		if (ym3812 == nullptr)
			return;

		ym3812->write(address, data);
	}

	DllExport void ym2413_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		ym2413_device *ym2413 = dynamic_cast<ym2413_device *>(rm->device((std::string("ym2413_") + num).c_str()));
		//ym2413_device *ym2413 = dynamic_cast<ym2413_device *>(rm->root_device().subdevice((std::string("ym2413_") + num).c_str()));
		if (ym2413 == nullptr)
			return;

		ym2413->write(address, data);
	}

	DllExport void gb_apu_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
		//dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->root_device().subdevice((std::string("gbsnd_") + num).c_str()));
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
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return 0;

		std::string num = std::to_string(unitNumber);
		dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
		//dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->root_device().subdevice((std::string("gbsnd_") + num).c_str()));
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
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
		//dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->root_device().subdevice((std::string("gbsnd_") + num).c_str()));
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
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		sn76496_base_device *sn76496 = dynamic_cast<sn76496_base_device *>(rm->device((std::string("sn76496_") + num).c_str()));
		//sn76496_base_device *sn76496 = dynamic_cast<sn76496_base_device *>(rm->root_device().subdevice((std::string("sn76496_") + num).c_str()));
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
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->device((std::string("namco_cus30_") + num).c_str()));
		//namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->root_device().subdevice((std::string("namco_cus30_") + num).c_str()));
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
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return 0;

		std::string num = std::to_string(unitNumber);
		namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->device((std::string("namco_cus30_") + num).c_str()));
		//namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->root_device().subdevice((std::string("namco_cus30_") + num).c_str()));
		if (cus30 == nullptr)
			return 0;

		return cus30->namcos1_cus30_r(*dummy, address);
	}

	DllExport void nes_apu_regwrite(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->device((std::string("nes_apu_") + num).c_str()));
		//nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->root_device().subdevice((std::string("nes_apu_") + num).c_str()));
		if (nesapu == nullptr)
			return;

		nesapu->write(address, data);
	}

	DllExport unsigned char nes_apu_regread(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return 0;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return 0;

		std::string num = std::to_string(unitNumber);
		nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->device((std::string("nes_apu_") + num).c_str()));
		//nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->root_device().subdevice((std::string("nes_apu_") + num).c_str()));
		if (nesapu == nullptr)
			return 0;

		return nesapu->read(address);
	}

	DllExport void nes_apu_set_dpcm(unsigned int unitNumber, unsigned char* address, unsigned int length)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->device((std::string("nes_apu_") + num).c_str()));
		//nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->root_device().subdevice((std::string("nes_apu_") + num).c_str()));
		if (nesapu == nullptr)
			return;

		nesapu->set_dpcm(address, length);
	}

	DllExport void SCC1_waveform_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
		//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
		if (scc1 == nullptr)
			return;

		scc1->k052539_waveform_w(address, data);
	}


	DllExport void SCC1_volume_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
		//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
		if (scc1 == nullptr)
			return;

		scc1->k051649_volume_w(address, data);
	}

	DllExport void SCC1_frequency_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
		//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
		if (scc1 == nullptr)
			return;

		scc1->k051649_frequency_w(address, data);
	}

	DllExport void SCC1_keyonoff_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
		//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
		if (scc1 == nullptr)
			return;

		scc1->k051649_keyonoff_w(data);
	}

	DllExport unsigned char SCC1_keyonoff_r(unsigned int unitNumber, unsigned int address)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return 0;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return 0;

		std::string num = std::to_string(unitNumber);
		k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
		//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
		if (scc1 == nullptr)
			return 0;

		return scc1->k051649_keyonoff_r();
	}

	DllExport void msm5232_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->device((std::string("msm5232_") + num).c_str()));
		//msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->root_device().subdevice((std::string("msm5232_") + num).c_str()));
		if (msm5232 == nullptr)
			return;

		msm5232->write(*dummy, address, data);
	}

	DllExport void msm5232_set_volume(unsigned int unitNumber, unsigned int ch, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->device((std::string("msm5232_") + num).c_str()));
		//msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->root_device().subdevice((std::string("msm5232_") + num).c_str()));
		if (msm5232 == nullptr)
			return;

		msm5232->set_volume(ch, data);
	}

	DllExport void msm5232_set_capacitors(unsigned int unitNumber, double cap1, double cap2, double cap3, double cap4, double cap5, double cap6, double cap7, double cap8)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->device((std::string("msm5232_") + num).c_str()));
		//msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->root_device().subdevice((std::string("msm5232_") + num).c_str()));
		if (msm5232 == nullptr)
			return;

		msm5232->set_capacitors(cap1, cap2, cap3, cap4, cap5, cap6, cap7, cap8);
	}

	DllExport void ay8910_address_data_w(unsigned int unitNumber, int offset, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->device((std::string("ay8910_") + num).c_str()));
		//ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->root_device().subdevice((std::string("ay8910_") + num).c_str()));
		if (ay8910 == nullptr)
			return;

		ay8910->address_data_w(offset, data);
	}

	DllExport unsigned char ay8910_read_ym(unsigned int unitNumber)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return 0;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return 0;

		std::string num = std::to_string(unitNumber);
		ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->device((std::string("ay8910_") + num).c_str()));
		//ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->root_device().subdevice((std::string("ay8910_") + num).c_str()));
		if (ay8910 == nullptr)
			return 0;

		return ay8910->data_r();
	}


	DllExport void mos8580_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		mos8580_device *mos8580 = dynamic_cast<mos8580_device*>(rm->device((std::string("mos8580_") + num).c_str()));
		//mos8580_device *mos8580 = dynamic_cast<mos8580_device*>(rm->root_device().subdevice((std::string("mos8580_") + num).c_str()));
		if (mos8580 == nullptr)
			return;

		mos8580->write(address, data);
	}

	DllExport void mos6581_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		mos6581_device *mos6581 = dynamic_cast<mos6581_device*>(rm->device((std::string("mos6581_") + num).c_str()));
		//mos6581_device *mos6581 = dynamic_cast<mos6581_device*>(rm->root_device().subdevice((std::string("mos6581_") + num).c_str()));
		if (mos6581 == nullptr)
			return;

		mos6581->write(address, data);
	}

	DllExport void beep_set_clock(unsigned int unitNumber, int state, unsigned int frequency)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		beep_device *beep = dynamic_cast<beep_device *>(rm->device((std::string("beep_") + num).c_str()));
		//beep_device *beep = dynamic_cast<beep_device *>(rm->root_device().subdevice((std::string("beep_") + num).c_str()));
		if (beep == nullptr)
			return;

		if (frequency != 0)
			beep->set_clock(frequency);
		beep->set_state(state);
	}


	DllExport void c140_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		c140_device *c140 = dynamic_cast<c140_device *>(rm->device((std::string("c140_") + num).c_str()));
		//c140_device *c140 = dynamic_cast<c140_device *>(rm->root_device().subdevice((std::string("c140_") + num).c_str()));
		if (c140 == nullptr)
			return;

		c140->c140_w(address, data);
	}


	DllExport void c140_set_callback(unsigned int unitNumber, C140_CALLBACK callback)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		c140_device *c140 = dynamic_cast<c140_device *>(rm->device((std::string("c140_") + num).c_str()));
		//c140_device *c140 = dynamic_cast<c140_device *>(rm->root_device().subdevice((std::string("c140_") + num).c_str()));
		if (c140 == nullptr)
			return;

		c140->set_callback(callback);
	}
}


