Ped = mp.peds.new(mp.game.joaat('a_m_m_prolhost_01'), new mp.Vector3(228.7092, 374.3901, 106.10), 162.0874, (streamPed) => { }, mp.players.local.dimension);
mp.peds.new(1767447799, new mp.Vector3(227.50, 379.20, 105.65), 251.5835); // ped
global.School = mp.browsers.new('package://game_resources/interface/DrivingSchool.html'); //статистика

mp.events.add('OpenDrivingSchool', (m0, m1, m2, m3, m4, m5) => {
  if (global.menuCheck()) return;
  School.execute(`DrivingSchool.set('${m0}','${m1}','${m2}','${m3}','${m4}','${m5}')`);
  School.execute('DrivingSchool.active=1');
  global.menuOpen();
  let sceneryCamera = mp.cameras.new('default', new mp.Vector3(231, 378, 107), new mp.Vector3(0, 0, 0), 40);
  sceneryCamera.pointAtCoord(221, 382, 106);
  sceneryCamera.setActive(true);
  mp.game.cam.renderScriptCams(true, false, 0, true, false);
});
mp.events.add('CloseDrivingSchool', () => {
  School.execute('DrivingSchool.active=0');
  global.menuClose();
  mp.game.cam.renderScriptCams(false, false, 500, true, false);
});
mp.events.add("selectSchool", (id) => {
  mp.events.callRemote("selectSchool_ID", id);
});


mp.events.add('OpenStatsDrivingSchool', (minCarHe, bodyHealth, engineHealth) => {
  School.execute('StatsDrivingSchool.active=1');
  School.execute(`StatsDrivingSchool.set('${minCarHe}', '${bodyHealth}', '${engineHealth}')`);
});
mp.events.add('CloseStatsDrivingSchool', () => {
  School.execute('StatsDrivingSchool.active=0');
});


mp.events.add('SetMaxSpeedSchool', () => {
  let vehicle = mp.players.local.vehicle;
  let speed = 16.66;
  vehicle.setMaxSpeed(speed);
});


mp.events.add('DrivingSchoolTEST', () => {
  if (global.menuCheck()) return;
  School.execute(`DrivingSchoolTEST.set()`);
  School.execute('DrivingSchoolTEST.active=1');
  global.menuOpen();
});
mp.events.add('CloseDrivingSchoolTEST', () => {
  School.execute('DrivingSchoolTEST.active=0');
  global.menuClose();
});

mp.events.add("selectSchoolTEST", (ok) => {
  if (new Date().getTime() - global.lastCheck < 1000) return;
  global.lastCheck = new Date().getTime();
  mp.events.callRemote("SelectSchoolOK", ok);
});

