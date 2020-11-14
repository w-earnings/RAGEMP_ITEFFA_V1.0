const localPlayer = mp.players.local;
let autoPilotActivated = false;
let autoPilotColshape = null;
const vehiclesHaveAutopilot = ["teslax"];

mp.keys.bind(0x73, true, function() { // F4
	if(localPlayer.vehicle)
	{
        for (let k = 0; k < vehiclesHaveAutopilot.length; k++)
        {
            let model = mp.game.joaat(vehiclesHaveAutopilot[k]);

            if (!localPlayer.vehicle.isModel(model))
                return mp.game.graphics.notify("Данный транспорт не оснащен системой автопилота");
        }

		if(localPlayer.vehicle.getPedInSeat(-1) == localPlayer.handle && autoPilotActivated == false)
		{
            if(localPlayer.vehicle.getIsEngineRunning() == false)
            {
                mp.events.call("Сначала заведите двигатель");
                return;
            }

			if(mp.game.invoke('0x1DD1F58F493F1DA5')) // IS_WAYPOINT_ACTIVE
			{
				let blipID 		= mp.game.invoke('0x186E5D252FA50E7D'); 		// _GET_BLIP_INFO_ID_ITERATOR
				let firstBlip 	= mp.game.invoke('0x1BEDE233E6CD2A1F', blipID); // GET_FIRST_BLIP_INFO_ID
				let nextBlip 	= mp.game.invoke('0x14F96AA50D6FBEA7', blipID); // GET_NEXT_BLIP_INFO_ID

				for(let i = firstBlip; mp.game.invoke('0xA6DB27D19ECBB7DA', i) != 0; i = nextBlip) // DOES_BLIP_EXIST
				{
					if(mp.game.invoke('0xBE9B0959FFD0779B', i) == 4) // GET_BLIP_INFO_ID_TYPE
					{
						let coord = mp.game.ui.getBlipInfoIdCoord(i);
						
						if(coord) 
						{
							localPlayer.taskVehicleDriveToCoordLongrange(localPlayer.vehicle.handle, coord.x, coord.y, 0.0, 22.0, 2883621, 40.0);

                            mp.game.graphics.notify("~g~Автопилот активирован");

							autoPilotActivated = true;

							if(autoPilotColshape != null) autoPilotColshape.destroy();

							autoPilotColshape = mp.colshapes.newCircle(coord.x, coord.y, 15.0, localPlayer.dimension);
						}

						return ;
					}
				}
			}

            mp.game.graphics.notify("Поставьте метку на карте и активируйте автопилот");
        }
        else if (autoPilotActivated)
        {
            stopAutopilot(false);
            mp.game.graphics.notify("~r~Автопилот деактивирован");
        }
	}
});

mp.events.add('playerEnterVehicle', () => {
	if(localPlayer.vehicle) 
		stopAutopilot(false);
});

mp.events.add('vehicleEngineHandler', () => {
	if(localPlayer.vehicle) 
		stopAutopilot();
});

mp.events.add("playerEnterColshape", (shape) => {

	if(shape == autoPilotColshape) 
	{
        stopAutopilot();
        mp.game.graphics.notify("~g~Вы прибыли на место назначения");
	}
});

function stopAutopilot(stopVehicle = true)
{
	if(autoPilotActivated)
	{
		if(localPlayer.vehicle)
		{
			if(stopVehicle) localPlayer.vehicle.setVelocity(0.0, 0.0, 0.0);
			localPlayer.clearTasks();
		}

		if(autoPilotColshape != null)
		{
			autoPilotColshape.destroy();
			autoPilotColshape = null;
		}

		autoPilotActivated = false;
	}
}