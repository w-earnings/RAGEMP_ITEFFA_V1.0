global.menuJobs = mp.browsers.new('package://game_resources/interface/working/farmer/index.html');
mp.events.add("openJobsMenu", (json) => {
  if (!loggedin || chatActive || editing || cuffed) return;
  global.menuOpen();
  global.menuJobs.active = true;
  setTimeout(function () {
    global.menuJobs.execute(`menuJobs.active=true`);
    global.menuJobs.execute(`menuJobs.setinfo(${json})`);
  }, 250);
});
mp.events.add("closeJobMenu", () => {
  setTimeout(function () {
    global.menuClose();
    global.menuJobs.active = false;
  }, 100);
});
mp.events.add("changeWorkState", (state, name) => {
  mp.events.callRemote("workstate", state, name);
});
var plantBlips = [];
var plantMarkers = [];
mp.events.add('createPlant', function (uid, names, type, position, scale, dimension, r, g, b) {
  try {
    if (typeof plantBlips[uid] != "undefined") {
      plantBlips[uid].destroy();
      plantBlips[uid] = undefined;
    }
    if (typeof plantMarkers[uid] != "undefined") {
      plantMarkers[uid].destroy();
      plantMarkers[uid] = undefined;
    }
    plantBlips[uid] = mp.blips.new(type, position, {
      name: names,
      scale: 1,
      color: 4,
      alpha: 255,
      drawDistance: 100,
      shortRange: false,
      rotation: 0,
      dimension: dimension
    });
    plantMarkers[uid] = mp.markers.new(type, position, scale, {
      visible: true,
      dimension: dimension,
      color: [r, g, b, 255]
    });
  } catch {}
});
mp.events.add('deletePlant', function (uid) {
  if (typeof plantBlips[uid] == "undefined") return;
  plantBlips[uid].destroy();
  plantBlips[uid] = undefined;
  if (typeof plantMarkers[uid] == "undefined") return;
  plantMarkers[uid].destroy();
  plantMarkers[uid] = undefined;
});