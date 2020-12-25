let waypoint;
let lastWaypointCoords;
let iTeffaToggle = false;

function findZ(mp, maxAttempts, delay, wpos, oldpos) {
    mp.players.local.position = new mp.Vector3(wpos.x, wpos.y, 0);
    mp.players.local.freezePosition(true);
    attempts = 1;

    timeout = setTimeout(function getZ() {
        wpos.z = mp.game.gameplay.getGroundZFor3dCoord(wpos.x, wpos.y, 1000, 0, false);
        if (!wpos.z && attempts < 10){
            attempts++;
            mp.players.local.position = new mp.Vector3(wpos.x, wpos.y, attempts*50);
            timeout = setTimeout(getZ, delay) 
        } else if(!wpos.z && attempts == maxAttempts) {
            mp.players.local.position = oldpos;
            mp.game.graphics.notify(`Ошибка: ~n~~h~~r~Не удалось получить координату Z.`);
            mp.players.local.freezePosition(false);
            clearTimeout(timeout);
        } else {
            mp.players.local.position = new mp.Vector3(wpos.x, wpos.y, wpos.z+2);
            mp.players.local.freezePosition(false);
            mp.events.callRemote('notifyCoords', 'Телепорт пo координатам:', wpos.x, wpos.y, wpos.z+1);
            clearTimeout(timeout);
        }
    }, delay)
}

function findWP(mp){
    let wpos = Object.assign({}, lastWaypointCoords);
    let oldpos = mp.players.local.position;

    if (wpos.z != 20) {
        mp.players.local.position = new mp.Vector3(wpos.x, wpos.y, wpos.z+2);
        mp.events.callRemote('notifyCoords', 'Телепорт по координатам:', wpos.x, wpos.y, wpos.z+1);
        return;
    }
    findZ(mp, 10, 150, wpos, oldpos);
}

mp.events.add('iTeffaToggle', () => {
    iTeffaToggle = !iTeffaToggle;
    if (iTeffaToggle) status=`~h~~g~Включен.`; else status=`~h~~r~Выключен.`;
    mp.game.graphics.notify(`WayPonitTeleport: ~n~${status}`);
});

mp.events.add('tpToWaypoint', () => {
    findWP(mp);
});

mp.events.add('render', () => {
    if(waypoint !== mp.game.invoke('0x1DD1F58F493F1DA5')){
        waypoint = mp.game.invoke('0x1DD1F58F493F1DA5'); 
        if (waypoint) {
            let blip = mp.game.invoke('0x1BEDE233E6CD2A1F', 8);
            let coords = mp.game.ui.getBlipInfoIdCoord(blip);
            lastWaypointCoords = coords;
            if (iTeffaToggle) mp.events.call('tpToWaypoint');
        }  
    }
});

mp.keys.bind(0x59, true, function() {
if (!loggedin || chatActive || editing || global.menuCheck() || cuffed || localplayer.getVariable('InDeath') == true) return;
    if (!global.localplayer.getVariable('IS_ADMIN')) return;
    if(!lastWaypointCoords){mp.game.graphics.notify(`Ошибка: ~n~~h~~r~Нет записи последнего waypoint'a.`); return;}
    mp.events.call('tpToWaypoint');
});

mp.keys.bind(0x71, true, function() {
if (!loggedin || chatActive || editing || global.menuCheck() || cuffed || localplayer.getVariable('InDeath') == true) return;
    if (!global.localplayer.getVariable('IS_ADMIN')) return;
    mp.events.call('iTeffaToggle');
});
