var cam = mp.cameras.new('default', new mp.Vector3(0, 0, 0), new mp.Vector3(0, 0, 0), false);
mp.peds.new(0x94562DD7, new mp.Vector3(2367.39, 4881.526, 42), 120, 0);    // Работа фермер
mp.peds.new(0xEF154C47, new mp.Vector3(438.3554, 6510.949, 28.6), 90, 0);  // Работа фермер
var effect = '';
global.loggedin = false;
global.lastCheck = 0;
global.chatLastCheck = 0;
global.pocketEnabled = false;
var Peds = [
  {
    Hash: -1988720319,
    Pos: new mp.Vector3(-1290.61, -574.38, 30.57),
    Angle: 260.92
  }, // Реалторское агенство
  {
    Hash: 1055701597,
    Pos: new mp.Vector3(-1030.60, -2744.5, 13.85),
    Angle: 21.000
  }, // Информация о сервере
  {
    Hash: -356333586,
    Pos: new mp.Vector3(1695.806, 43.05446, 161.7473),
    Angle: 99.60
  }, // Работа: Дайвер
  {
    Hash: 1644266841,
    Pos: new mp.Vector3(144.8581, -373.5612, 43.65),
    Angle: 35.74032
  }, // Работа грузчик
  // Банды
  {
    Hash: 1706635382,
    Pos: new mp.Vector3(-20.23, -1413.89, 29.31, 272.53),
    Angle: 272.53
  }, // The Families
  {
    Hash: 588969535,
    Pos: new mp.Vector3(122.29, -1997.97, 18.40),
    Angle: 284.86
  }, // The Ballas
  {
    Hash: -812470807,
    Pos: new mp.Vector3(475.67, -1892.16, 26.09),
    Angle: 302.48
  }, // Los Santos Vagos
  {
    Hash: 653210662,
    Pos: new mp.Vector3(1435.11, -1491.59, 63.62),
    Angle: 165.81
  }, // Marabunta Grande
  {
    Hash: 663522487,
    Pos: new mp.Vector3(965.93, -1828.77, 31.22),
    Angle: 355.79
  }, // Blood Street
  // Банки
  {
    Hash: 826475330,
    Pos: new mp.Vector3(148.1068, -1041.578, 29.44),
    Angle: 337.6359
  }, {
    Hash: -1280051738,
    Pos: new mp.Vector3(149.4417, -1042.151, 29.44),
    Angle: 337.6359
  }, {
    Hash: 826475330,
    Pos: new mp.Vector3(-1211.969, -331.9472, 37.86),
    Angle: 23.93865
  }, {
    Hash: -1280051738,
    Pos: new mp.Vector3(-1213.293, -332.6205, 37.86),
    Angle: 23.93865
  }, {
    Hash: 826475330,
    Pos: new mp.Vector3(312.3441, -279.9223, 54.24),
    Angle: 338.3335
  }, {
    Hash: -1280051738,
    Pos: new mp.Vector3(313.7816, -280.4292, 54.24),
    Angle: 338.3335
  }, {
    Hash: 826475330,
    Pos: new mp.Vector3(1176.533, 2708.258, 38.10),
    Angle: 178.4438
  }, {
    Hash: -1280051738,
    Pos: new mp.Vector3(1175.001, 2708.257, 38.10),
    Angle: 178.4438
  }, {
    Hash: 826475330,
    Pos: new mp.Vector3(-110.1725, 6468.931, 31.70),
    Angle: 128.8161
  }, {
    Hash: -1280051738,
    Pos: new mp.Vector3(-112.3225, 6471.084, 31.70),
    Angle: 128.8161
  },
  // Перебрать
  {
    Hash: -39239064,
    Pos: new mp.Vector3(1395.184, 3613.144, 34.9892),
    Angle: 270.0
  }, // Caleb Baker
  {
    Hash: -1176698112,
    Pos: new mp.Vector3(166.6278, 2229.249, 90.73845),
    Angle: 47.0
  }, // Matthew Allen
  {
    Hash: 1161072059,
    Pos: new mp.Vector3(2887.687, 4387.17, 50.65578),
    Angle: 174.0
  }, // Owen Nelson
  {
    Hash: -1398552374,
    Pos: new mp.Vector3(2192.614, 5596.246, 53.75177),
    Angle: 318.0
  }, // Daniel Roberts
  {
    Hash: -459818001,
    Pos: new mp.Vector3(-215.4299, 6445.921, 31.30351),
    Angle: 262.0
  }, // Michael Turner
  {
    Hash: 0x9D0087A8,
    Pos: new mp.Vector3(480.9385, -1302.576, 29.24353),
    Angle: 224.0
  }, // jimmylishman
  {
    Hash: 645279998,
    Pos: new mp.Vector3(-113.9224, 985.793, 235.754),
    Angle: 110.9234
  }, // Vladimir_Medvedev
  {
    Hash: -236444766,
    Pos: new mp.Vector3(-1811.368, 438.4105, 128.7074),
    Angle: 348.107
  }, // Kaha_Panosyan
  {
    Hash: -1427838341,
    Pos: new mp.Vector3(-1549.287, -89.35114, 54.92917),
    Angle: 7.874235
  }, // Jotaro_Josuke
  {
    Hash: -2034368986,
    Pos: new mp.Vector3(1392.098, 1155.892, 114.4433),
    Angle: 82.24557
  }, // Solomon_Gambino
  {
    Hash: -1920001264,
    Pos: new mp.Vector3(452.2527, -993.119, 30.68958),
    Angle: 357.7483
  }, // Alonzo_Harris
  {
    Hash: 368603149,
    Pos: new mp.Vector3(441.169, -978.3074, 30.6896),
    Angle: 160.1411
  }, // Nancy_Spungen
  {
    Hash: 1581098148,
    Pos: new mp.Vector3(454.121, -980.0575, 30.68959),
    Angle: 86.12
  }, // Bones_Bulldog
  {
    Hash: 941695432,
    Pos: new mp.Vector3(149.1317, -758.3485, 242.152),
    Angle: 66.82055
  }, //  Steve_Hain
  {
    Hash: 1558115333,
    Pos: new mp.Vector3(120.0836, -726.7773, 242.152),
    Angle: 248.3546
  }, // Michael Bisping
  {
    Hash: 1925237458,
    Pos: new mp.Vector3(-2347.958, 3268.936, 32.81076),
    Angle: 240.8822
  }, // Ronny_Pain
  {
    Hash: 988062523,
    Pos: new mp.Vector3(253.9357, 228.9332, 101.6832),
    Angle: 250.3564
  }, // Anthony_Young
  {
    Hash: 2120901815,
    Pos: new mp.Vector3(262.7953, 220.5285, 101.6832),
    Angle: 337.26
  }, // Lorens_Hope
  {
    Hash: 826475330,
    Pos: new mp.Vector3(247.6933, 219.5379, 106.2869),
    Angle: 65.78249
  }, // Heady_Hunter
  {
    Hash: -907676309,
    Pos: new mp.Vector3(724.8585, 134.1029, 80.95643),
    Angle: 245.0083
  }, // Ronny_Bolls
  {
    Hash: 940330470,
    Pos: new mp.Vector3(458.7059, -995.118, 25.35196),
    Angle: 176.8092
  }, // Rashkovsky
  {
    Hash: 1596003233,
    Pos: new mp.Vector3(459.7471, -1000.333, 24.91329),
    Angle: 177.2829
  }, // Muscle Prisoner
  {
    Hash: -520477356,
    Pos: new mp.Vector3(-455.9738, 6014.119, 31.59654),
    Angle: 357.7483
  }, // Bot
  {
    Hash: -1614285257,
    Pos: new mp.Vector3(-449.8658, 6012.458, 31.59655),
    Angle: 308.1411
  }, // Kira
  {
    Hash: -1699520669,
    Pos: new mp.Vector3(-429.0482, 5997.3, 31.59655),
    Angle: 86.12
  }, // Stepa
];
setTimeout(function () {
  Peds.forEach(ped => {
    mp.peds.new(ped.Hash, ped.Pos, ped.Angle, 0);
  });
}, 10000);
mp.game.gameplay.disableAutomaticRespawn(true);
mp.game.gameplay.ignoreNextRestart(true);
mp.game.gameplay.setFadeInAfterDeathArrest(false);
mp.game.gameplay.setFadeOutAfterDeath(false);
mp.game.gameplay.setFadeInAfterLoad(false);
mp.events.add('freeze', function (toggle) {
  localplayer.freezePosition(toggle);
});
mp.events.add('destroyCamera', function () {
  cam.destroy();
  mp.game.cam.renderScriptCams(false, false, 3000, true, true);
});
mp.events.add('carRoom', function () {
  cam = mp.cameras.new('default', new mp.Vector3(-42.3758, -1101.672, 27.52235), new mp.Vector3(0, 0, 0), 50);
  cam.pointAtCoord(-42.79771, -1095.676, 26.0117);
  cam.setActive(true);
  mp.game.cam.renderScriptCams(true, false, 0, true, false);
});
mp.events.add('screenFadeOut', function (duration) {
  mp.game.cam.doScreenFadeOut(duration);
});
mp.events.add('screenFadeIn', function (duration) {
  mp.game.cam.doScreenFadeIn(duration);
});
var lastScreenEffect = "";
mp.events.add('startScreenEffect', function (effectName, duration, looped) {
  try {
    lastScreenEffect = effectName;
    mp.game.graphics.startScreenEffect(effectName, duration, looped);
  } catch (e) {}
});
mp.events.add('stopScreenEffect', function (effectName) {
  try {
    var effect = (effectName == undefined) ? lastScreenEffect : effectName;
    mp.game.graphics.stopScreenEffect(effect);
  } catch (e) {}
});
mp.events.add('stopAndStartScreenEffect', function (stopEffect, startEffect, duration, looped) {
  try {
    mp.game.graphics.stopScreenEffect(stopEffect);
    mp.game.graphics.startScreenEffect(startEffect, duration, looped);
  } catch (e) {}
});
mp.events.add('setHUDVisible', function (arg) {
  mp.game.ui.displayHud(arg);
  mp.gui.chat.show(arg);
  mp.game.ui.displayRadar(arg);
});
mp.events.add('setPocketEnabled', function (state) {
  pocketEnabled = state;
  if (state) {
    mp.gui.execute("fx.set('inpocket')");
    mp.game.invoke(getNative("SET_FOLLOW_PED_CAM_VIEW_MODE"), 4);
  } else {
    mp.gui.execute("fx.reset()");
  }
});
mp.keys.bind(global.Keys.VK_F3, false, function () {
  mp.events.callRemote('reloadcef');
});

mp.keys.bind(Keys.VK_N, false, function () {
  if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
  mp.events.callRemote('cancelPressed');
  lastCheck = new Date().getTime();
});
mp.events.add('connected', function () {
  mp.game.ui.displayHud(false);
  cam = mp.cameras.new('default', startCamPos, startCamRot, 90.0);
  cam.setActive(true);
  mp.game.graphics.startScreenEffect('SwitchSceneMichael', 5000, false);
  var effect = 'SwitchSceneMichael';
});
mp.events.add('ready', function () {
  mp.game.ui.displayHud(true);
});
mp.events.add('kick', function (notify) {
  mp.events.call('notify', 4, 9, notify, 10000);
  mp.events.callRemote('kickclient');
});
mp.events.add('loggedIn', function () {
  loggedin = true;
});
mp.events.add('setFollow', function (toggle, entity) {
  if (toggle) {
    if (entity && mp.players.exists(entity)) localplayer.taskFollowToOffsetOf(entity.handle, 0, 0, 0, 1, -1, 1, true)
  } else localplayer.clearTasks();
});
setInterval(function () {
  if (localplayer.getArmour() <= 0 && localplayer.getVariable('HASARMOR') === true) {
    mp.events.callRemote('deletearmor');
  }
}, 600);
mp.keys.bind(Keys.VK_U, false, function () { // Animations selector
  if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
  if (localplayer.isInAnyVehicle(true)) return;
  OpenCircle("Категории", 0);
});
mp.keys.bind(Keys.VK_Y, false, function () { // Телепорт
  if (!loggedin || chatActive || editing || global.menuCheck() || cuffed || localplayer.getVariable('InDeath') == true) return;
  if (!global.localplayer.getVariable('IS_ADMIN')) return;
  GoPosPLS();
  lastCheck = new Date().getTime();
});
mp.keys.bind(Keys.VK_E, false, function () { // E key
  if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
  mp.events.callRemote('interactionPressed');
  lastCheck = new Date().getTime();
  global.acheat.pos();
});
mp.keys.bind(Keys.VK_L, false, function () { // L key
  if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
  mp.events.callRemote('lockCarPressed');
  lastCheck = new Date().getTime();
});
mp.keys.bind(Keys.VK_LEFT, true, () => {
  if (mp.gui.cursor.visible || !loggedin) return;
  if (localplayer.vehicle) {
    if (localplayer.vehicle.getPedInSeat(0) != localplayer.handle) return;
    if (new Date().getTime() - lastCheck > 500) {
      lastCheck = new Date().getTime();
      if (localplayer.vehicle.getVariable('leftlight') == true) mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
      else mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 1, 0);
    }
  }
});
mp.keys.bind(Keys.VK_RIGHT, true, () => {
  if (mp.gui.cursor.visible || !loggedin) return;
  if (localplayer.vehicle) {
    if (localplayer.vehicle.getPedInSeat(0) != localplayer.handle) return;
    if (new Date().getTime() - lastCheck > 500) {
      lastCheck = new Date().getTime();
      if (localplayer.vehicle.getVariable('rightlight') == true) mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
      else mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 1);
    }
  }
});
mp.keys.bind(Keys.VK_DOWN, true, () => {
  if (mp.gui.cursor.visible || !loggedin) return;
  if (localplayer.vehicle) {
    if (localplayer.vehicle.getPedInSeat(0) != localplayer.handle) return;
    if (new Date().getTime() - lastCheck > 500) {
      lastCheck = new Date().getTime();
      if (localplayer.vehicle.getVariable('leftlight') == true && localplayer.vehicle.getVariable('rightlight') == true) mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
      else mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 1, 1);
    }
  }
});
mp.keys.bind(Keys.VK_B, false, function () { // 2 key
  if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 400 || global.menuOpened) return;
  if (localplayer.isInAnyVehicle(false) && localplayer.vehicle.getSpeed() <= 3) {
    lastCheck = new Date().getTime();
    mp.events.callRemote('engineCarPressed');
  }
});
mp.keys.bind(Keys.VK_M, false, function () {
  if (!loggedin || chatActive || editing || global.menuCheck() || cuffed || localplayer.getVariable('InDeath') == true || new Date().getTime() - lastCheck < 400) return;
  if (global.phoneOpen) {
    mp.game.invoke('0x3BC861DF703E5097', mp.players.local.handle, true);
    mp.events.callRemote("closePlayerMenu");
    global.phoneOpen = 0;
  } else {
    mp.events.callRemote('openPlayerMenu');
    mp.game.mobile.createMobilePhone(3);
    mp.game.mobile.setMobilePhoneScale(0);
    mp.game.mobile.scriptIsMovingMobilePhoneOffscreen(false);
    mp.game.mobile.setPhoneLean(false);
    lastCheck = new Date().getTime();
    global.phoneOpen = 1;
  }
});
mp.keys.bind(0x77, true, function () { //F8-Key
  var date = new Date();
  var name = "sanstreetlife-" + date.getDate() + "." + date.getMonth() + "." + date.getFullYear() + "-" + date.getHours() + "." + date.getMinutes() + "." + date.getSeconds() + ".png";
  mp.gui.takeScreenshot(name, 1, 100, 0);
});
mp.keys.bind(Keys.VK_X, false, function () { // X key
  if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
  mp.events.callRemote('playerPressCuffBut');
  lastCheck = new Date().getTime();
});
mp.keys.bind(Keys.VK_Z, false, function () { // Z key
  if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
  if (localplayer.vehicle) {
    if (localplayer.vehicle.getPedInSeat(0) != localplayer.handle) CheckMyWaypoint();
    else {
      if (localplayer.vehicle.getClass() == 18) mp.events.callRemote('syncSirenSound', localplayer.vehicle);
    }
  } else mp.events.callRemote('playerPressFollowBut');
  lastCheck = new Date().getTime();
});

function CheckMyWaypoint() {
  try {
    if (mp.game.invoke('0x1DD1F58F493F1DA5')) {
      let foundblip = false;
      let blipIterator = mp.game.invoke('0x186E5D252FA50E7D');
      let totalBlipsFound = mp.game.invoke('0x9A3FF3DE163034E8');
      let FirstInfoId = mp.game.invoke('0x1BEDE233E6CD2A1F', blipIterator);
      let NextInfoId = mp.game.invoke('0x14F96AA50D6FBEA7', blipIterator);
      for (let i = FirstInfoId, blipCount = 0; blipCount != totalBlipsFound; blipCount++, i = NextInfoId) {
        if (mp.game.invoke('0x1FC877464A04FC4F', i) == 8) {
          var coord = mp.game.ui.getBlipInfoIdCoord(i);
          foundblip = true;
          break;
        }
      }
      if (foundblip) mp.events.callRemote('syncWaypoint', coord.x, coord.y);
    }
  } catch (e) {}
}

mp.events.add('syncWP', function (bX, bY, type) {
  if (!mp.game.invoke('0x1DD1F58F493F1DA5')) {
    mp.game.ui.setNewWaypoint(bX, bY);
    if (type == 0) mp.events.call('notify', 2, 9, "Пассажир передал Вам информацию о своём маршруте!", 3000);
    else if (type == 1) mp.events.call('notify', 2, 9, "Человек из списка контактов Вашего телефона передал Вам метку его местоположения!", 3000);
  } else {
    if (type == 0) mp.events.call('notify', 4, 9, "Пассажир попытался передать Вам информацию о маршруте, но у Вас уже установлен другой маршрут.", 5000);
    else if (type == 1) mp.events.call('notify', 4, 9, "Человек из списка контактов Вашего телефона попытался передать Вам метку его местоположения, но у Вас уже установлена другая метка.", 5000);
  }
});
mp.keys.bind(Keys.VK_U, false, function () { // U key
  if (!loggedin || chatActive || editing || global.menuOpened || new Date().getTime() - lastCheck < 1000) return;
  mp.events.callRemote('openCopCarMenu');
  lastCheck = new Date().getTime();
});
mp.keys.bind(Keys.VK_OEM_3, false, function () { // ` key
  if (chatActive || (global.menuOpened && mp.gui.cursor.visible)) return;
  mp.gui.cursor.visible = !mp.gui.cursor.visible;
});
var lastPos = new mp.Vector3(0, 0, 0);
mp.game.gameplay.setFadeInAfterDeathArrest(false);
mp.game.gameplay.setFadeInAfterLoad(false);
var deathTimerOn = false;
var deathTimer = 0;
mp.events.add('DeathTimer', (time) => {
  if (time === false) deathTimerOn = false;
  else {
    deathTimerOn = true;
    deathTimer = new Date().getTime() + time;
  }
});
mp.events.add('render', () => {
  if (localplayer.getVariable('InDeath') == true) {
    mp.game.controls.disableAllControlActions(2);
    mp.game.controls.enableControlAction(2, 1, true);
    mp.game.controls.enableControlAction(2, 2, true);
    mp.game.controls.enableControlAction(2, 3, true);
    mp.game.controls.enableControlAction(2, 4, true);
    mp.game.controls.enableControlAction(2, 5, true);
    mp.game.controls.enableControlAction(2, 6, true);
  }
  if (deathTimerOn) {
    var secondsLeft = Math.trunc((deathTimer - new Date().getTime()) / 1000);
    var minutes = Math.trunc(secondsLeft / 60);
    var seconds = secondsLeft % 60;
    mp.game.graphics.drawText(`До смерти осталось ${minutes}:${seconds}`, [0.5, 0.8], {
      font: 0,
      color: [255, 255, 255, 200],
      scale: [0.35, 0.35],
      outline: true
    });
  }
  if (mp.game.controls.isControlPressed(0, 32) || mp.game.controls.isControlPressed(0, 33) || mp.game.controls.isControlPressed(0, 321) || mp.game.controls.isControlPressed(0, 34) || mp.game.controls.isControlPressed(0, 35) || mp.game.controls.isControlPressed(0, 24) || localplayer.getVariable('InDeath') == true) {
    afkSecondsCount = 0;
  } else if (localplayer.isInAnyVehicle(false) && localplayer.vehicle.getSpeed() != 0) {
    afkSecondsCount = 0;
  } else if (spectating) {
    afkSecondsCount = 0;
  }
});
mp.events.add("playerRuleTriggered", (rule, counter) => {
  if (rule === 'ping' && counter > 5) {
    mp.events.call('notify', 4, 2, "Ваш ping слишком большой. Зайдите позже", 5000);
    mp.events.callRemote("kickclient");
  }
});