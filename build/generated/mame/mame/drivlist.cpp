#include "emu.h"

#include "drivenum.h"

GAME_EXTERN(___empty);
GAME_EXTERN(genesis);

const game_driver * const driver_list::s_drivers_sorted[2] =
{
	&GAME_NAME(___empty),
	&GAME_NAME(genesis),
};

std::size_t const driver_list::s_driver_count = 2;

