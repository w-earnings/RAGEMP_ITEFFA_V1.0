// HOUSE //
global.house = mp.browsers.new('package://game_resources/interface/houses.html');
// global.jobs = mp.browsers.new('package://cef/jobs.html');

mp.events.add('HouseMenu', (id, Owner, Type, Locked, Price, Garage, Roommates) => {
	if (global.menuCheck()) return;
    menuOpen();
	house.execute(`hm.set('${id}','${Owner}','${Type}','${Locked}','${Price}','${Garage}','${Roommates}')`);
	house.execute('hm.active=1');
	jobs.execute('JobsEinfo.active=0');
});

mp.events.add('HouseMenuBuy', (id, Owner, Type, Locked, Price, Garage, Roommates) => {
	if (global.menuCheck()) return;
    menuOpen();
	house.execute(`hmBuy.set('${id}','${Owner}','${Type}','${Locked}','${Price}','${Garage}','${Roommates}')`);
	house.execute('hmBuy.active=1');
	jobs.execute('JobsEinfo.active=0');
});

mp.events.add("GoHouseMenu", (id) => {
    mp.events.callRemote("GoHouseMenuS", id);
	house.execute('hm.active=0');
	jobs.execute('JobsEinfo.active=0');
	global.menuClose();
    mp.gui.cursor.visible = false;
});

mp.events.add('CloseHouseMenu', () => {
	house.execute('hm.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});
mp.events.add('CloseHouseMenuBuy', () => {
	house.execute('hmBuy.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});

mp.events.add("buyHouseMenu", (id) => {
    mp.events.callRemote("buyHouseMenuS", id);
	house.execute('hmBuy.active=0');
	jobs.execute('JobsEinfo.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});

mp.events.add("WarnHouse", (id) => {
    mp.events.callRemote("WarnHouseS", id);
});
mp.events.add("CarHouse", (id) => {
    mp.events.callRemote("CarHouseS", id);
});
mp.events.add("LockedHouse", (id) => {
    mp.events.callRemote("LockedHouseS", id);
});


mp.events.add("SellHome", (id) => {
mp.events.callRemote("SellHomeS", id);
	house.execute('hm.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});

mp.events.add("Interior", (id) => {
    mp.events.callRemote("GoHouseInterS", id);
	house.execute('hmBuy.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});
mp.events.add("Garage", (id) => {
    mp.events.callRemote("GoGarageS", id);
	house.execute('hmExit.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});

mp.events.add("ExitHouseMenu", () => {
	if (global.menuCheck()) return;
    menuOpen();
	house.execute('hmExit.active=1');
});

mp.events.add("exitHouse", () => {
	mp.events.callRemote("ExitHouseMenuE");
	house.execute('hmExit.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});


