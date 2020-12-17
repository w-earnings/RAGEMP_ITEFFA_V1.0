let phone;
var phoneWindow = null;
var phoneOppened = false;
mp.events.add('initPhone', () => {
    phone = mp.browsers.new('package://game_resources/interface/mobile.html');
});

mp.events.add('phoneShow', () => {
    phone.execute('show();');
    mp.gui.cursor.visible = true;
});
mp.events.add('phoneHide', () => {
    phone.execute('hide();');
    mp.gui.cursor.visible = false;
});
mp.events.add('phoneOpen', (data) => {
    var json = JSON.parse(data);
    var id = json[0];
    var canHome = json[3];
    var canBack = json[2];
    var items = JSON.stringify(json[1]);
    var exec = "open('" + id + "'," + canHome + "," + canBack + ",'"  + items + "');";
    phone.execute(exec);
});
mp.events.add('phoneChange', (ind, data) => {
    var exec = "change(" + ind + ",'" + data + "');";
    phone.execute(exec);
});
mp.events.add('phoneClose', () => {
    if(phone != null) phone.execute('reset();');
});

mp.events.add('phoneCallback', (itemid, event, data) => {
    mp.events.callRemote('Phone', 'callback', itemid, event, data);
});
mp.events.add('phoneNavigation', (btn) => {
    mp.events.callRemote('Phone', 'navigation', btn);
});
