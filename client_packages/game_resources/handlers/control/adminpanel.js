mp.keys.bind(0x77, false, function () {
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened || !localplayer.getVariable("IS_ADMIN")) return;
    mp.events.callRemote('openAdminPanel');
    lastCheck = new Date().getTime(); //9IP
});

mp.events.add("openAdminPanel", (json, json2) => {
  if (!loggedin || chatActive || editing || cuffed) return;
  global.adminPanel = mp.browsers.new('package://game_resources/interface/adminpanel.html');
  global.menuOpen();
  global.adminPanel.active = true;
  setTimeout(function() {
    global.adminPanel.execute(`admlist.active=true`);
    global.adminPanel.execute(`admlist.cmdlist=${json}`);
    global.adminPanel.execute(`admlist.items=${json2}`);
  }, 250);
});

mp.events.add("closeAdminPanel", () => {
  setTimeout(function() {
		global.menuClose();
		if(global.adminPanel)
		{
			global.adminPanel.active = false;
			global.adminPanel.destroy();
		}
	}, 100);
});

mp.events.add("getPlayerInfo", (id) => {
  mp.events.callRemote('getPlayerInfoToAdminPanel', id);
});
mp.events.add("loadPlayerInfo", (json) => {
  global.adminPanel.execute(`admlist.player=${json}`);
}); //9IP
