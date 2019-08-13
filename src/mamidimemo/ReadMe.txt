*What is MAmidiMEmo?

MAmidiMEmo is virtual MIDI instrument(s) for chiptune. Sound engine is MAME.
You can control various chip and make a sound via MIDI I/F.
So, you don't need to use dedicated tracker and so on anymore. You can use your favorite MIDI sequencer to make a sound.

MAmidiMEmo adopt multi timbre method. That mean you can play multi chords on MIDI 1ch if the chip supports.
e.g.) YM2151 has 8ch for all timbres, so you can play 8 chords on MIDI 1ch like a SC-55 and so on.
      NES APU has 2ch for square wave, so you can play 2 chords on MIDI 1ch when select a square wave timbre.
	  However, when you select a triangle wave timbre, you can play 1 chord on MIDI 1ch. Because NES APU has only 1ch for triangle wave.

*How to use MAmidiMEmo

1. Launch MAmidiMEmo.exe

2. Select MIDI I/F from toolbar. MAmidiMEmo recevie MIDI message from the selected MIDI I/F.

   Note: You can use the loopMIDI(http://www.tobias-erichsen.de/software/loopmidi.html) to send MIDI message from this PC to MAmidiMEmo.

3. You can add your favorite chips from toolbar menu [Instruments].

   Note: Currently supported chip is the following.
         YM2151, YM2612, SN76496, NAMCO CUS30, GB APU, NES APU
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
	 http://fpga.blog.shinobi.jp/fpga/おんげん！
	SN76489:
	 http://www.smspower.org/Development/SN76489
	 http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html

   *[Channels]
    Select which MIDI ch messages the chip receive.

5. Play MIDI file

6. (TBD)
   You can modify current environment and all timbre parameters via System Exclusive MIDI Message.

   SysEx format:

	YM2151:(TBD)
	YM2612:(TBD)
	NES APU:(TBD)
	GB APU:(TBD)
	NAMCO CUS30:(TBD)
	SN76489:(TBD)
   
