global.infoped = mp.browsers.new('package://game_resources/interface/infoped.html');

mp.events.add('openInfoMenu', () => {
	if (global.menuCheck()) return;
    menuOpen();
	infoped.execute('infoped.active=1');
	mp.events.call('toBlur', 200)
});

mp.events.add('CloseInfoMenu', () => {
	infoped.execute('infoped.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
	mp.events.call('fromBlur', 200)
});
