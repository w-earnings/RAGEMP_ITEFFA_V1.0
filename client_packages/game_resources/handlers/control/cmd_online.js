mp.keys.bind(Keys.VK_F8, false, function () {
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened || !localplayer.getVariable("IS_ADMIN")) return;
    mp.events.callRemote('openCmdOnline');
    lastCheck = new Date().getTime();
});

mp.events.add("openCmdOnline", (json, json2) => {
  if (!loggedin || chatActive || editing || cuffed) return;
  global.cmdOnline = mp.browsers.new('package://game_resources/interface/cmd_online.html');
  global.menuOpen();
  global.cmdOnline.active = true;
  setTimeout(function() {
    global.cmdOnline.execute(`admlist.active=true`);
    global.cmdOnline.execute(`admlist.cmdlist=${json}`);
    global.cmdOnline.execute(`admlist.items=${json2}`);
  }, 250);
});

mp.events.add("closeCmdOnline", () => {
  setTimeout(function() {
		global.menuClose();
		if(global.cmdOnline)
		{
			global.cmdOnline.active = false;
			global.cmdOnline.destroy();
		}
	}, 100);
});
