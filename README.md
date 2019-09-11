MAmidiMEmo V0.6.0.0 / Itoken (c)2019 / GPL-2.0

*** What is the MAmidiMEmo? ***

MAmidiMEmo is a virtual chiptune sound MIDI module using a MAME sound engine.
You can control various chips and make sound via MIDI I/F.
So, you don't need to use dedicated tracker and so on anymore. You can use your favorite MIDI sequencer to make a chip sound.

MAmidiMEmo adopts multi timbre method. That mean you can play multi chords on MIDI 1ch if the chip has multi ch.

e.g.) YM2151 has 8ch FM sounds, so you can play 8 chords on MIDI 1ch or sharing 8ch with MIDI 16ch like a SC-55 and so on.
      As well, NES APU has 2ch for square wave, so you can play 2 chords on MIDI 1ch when select a square wave timbre.
	  However, when you select a triangle wave timbre, you can play 1 chord on MIDI 1ch. Because NES APU has only 1ch for triangle wave.

*** How to use MAmidiMEmo ***

0. Requirements

	*H/W
		*CPU: Intel Core series CPU or equivalent, at least 2.0 GHz
		*MEM: at least 4GB
		*Sound: DirectSound capable sound card/onboard audio
		*MIDI: MIDI IN I/F
	*S/W
		*OS: Windows 7 SP1 or lator
		*Runtime: .NET Framework 4.7 or lator

1. Launch MAmidiMEmo.exe

   Note: You can change the value of audio latency, sampling rate and audio output interface by modifying the "mame.ini"
         PortAudio is a low latency sound engine. See http://www.portaudio.com/

2. Select MIDI I/F from toolbar. MAmidiMEmo will recevie MIDI message from the selected MIDI I/F.

   Note: You can use the loopMIDI (See http://www.tobias-erichsen.de/software/loopmidi.html) to send MIDI message from this PC to MAmidiMEmo.

3. Add your favorite chips from the [Instruments] menu on the toolbar.

   Note: Currently supported chip is the following.
         YM2151, YM2612, YM3812, YM2413, SN76496, NES APU, MSM5232(+TA7630), NAMCO CUS30, GB APU, SCC, AY-3-8910
   Note: You can add the chip up to 8 per same chip type and MAmidiMEmo eats more CPU power.

4. Select the chip from the left pane and configure the chip on the right pane.

   *[Timbres]
    You can edit sound character from this property. It's selected by "Program Change" MIDI message.
	Please refer the following articles or MAME sources to understand these timbre parameters.

	YM2151:
	 https://www16.atwiki.jp/mxdrv/pages/24.html
	YM2612:
	 https://www.plutiedev.com/ym2612-registers
	 http://www.smspower.org/maxim/Documents/YM2612
	NES APU:
	 http://hp.vector.co.jp/authors/VA042397/nes/apu.html
	 https://wiki.nesdev.com/w/index.php/APU
	 https://wiki.nesdev.com/w/index.php/APU_DMC
	GB APU:
	 http://bgb.bircd.org/pandocs.htm#soundcontrolregisters
	 https://gbdev.gg8.se/wiki/articles/Gameboy_sound_hardware
	 http://mydocuments.g2.xrea.com/
	 http://marc.rawer.de/Gameboy/Docs/GBCPUman.pdf
	 http://www.devrs.com/gb/files/hosted/GBSOUND.txt
	NAMCO CUS30:
     https://www.walkofmind.com/programming/pie/wsg3.htm
	 http://fpga.blog.shinobi.jp/fpga/Ç®ÇÒÇ∞ÇÒÅI
	SN76489:
	 http://www.smspower.org/Development/SN76489
	 http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html
	SCC:
	 http://bifi.msxnet.org/msxnet/tech/scc.html
	YM3812:
	 http://www.oplx.com/opl2/docs/adlib_sb.txt
	YM2413:
	 http://d4.princess.ne.jp/msx/datas/OPLL/YM2413AP.html#31
	 http://www.smspower.org/maxim/Documents/YM2413ApplicationManual
	 http://hp.vector.co.jp/authors/VA054130/yamaha_curse.html
	MSM5232(+TA7630):
	 http://www.citylan.it/wiki/images/3/3e/5232.pdf
	 http://sr4.sakura.ne.jp/acsound/taito/taito5232.html
	AY-3-8910:
	 http://ngs.no.coocan.jp/doc/wiki.cgi/TechHan?page=1%BE%CF+PSG%A4%C8%B2%BB%C0%BC%BD%D0%CE%CF
	 https://w.atwiki.jp/msx-sdcc/pages/45.html
	 http://f.rdw.se/AY-3-8910-datasheet.pdf

   *[Channels]
    Select which MIDI ch messages the chip receives.

5. Play MIDI file by youe favorite sequencer or player.
   Of course, you can connect your favrite keyboard to MAmidiMEmo for live performance.

   MAmidiMEmo currently supports the following MIDI messages.

    Note on/off and Velocity
    Program Number
    Pitch and PitchRange
    Volume and Expression
    Panpot
	Modulation, Modulation Depth, Modulation Range, Modulation Delay
	Portamento, Portamento Time

6. Also, you can set the following sound driver settings from Timbre settings.

   Arpeggio
   ADSR
   Effect(Pitch/Volue/Duty macro)

7. (TBD)
   You can modify current environment and all timbre parameters via System Exclusive MIDI Message.

   SysEx format:

	YM2151:(TBD)
	YM2612:(TBD)
	NES APU:(TBD)
	GB APU:(TBD)
	NAMCO CUS30:(TBD)
	SN76489:(TBD)
	:::
 
8. Donate for supporting the MAmidiMEmo

   [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SNQ9JE3JAQMNQ)

9. Changes

0.6.1.0 Changed to new sound timer engine for perfect sound timing
0.6.0.0 Added sound driver effects and portamento feature
0.5.0.0 Added several chips
0.1.0.0 First release
