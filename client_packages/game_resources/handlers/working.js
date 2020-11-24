global.jobs = mp.browsers.new('package://game_resources/interface/working.html');

var JobMenusBlip = [];
mp.events.add('JobMenusBlip', function (uid, type, position, names, dir) {
    if (typeof JobMenusBlip[uid] != "undefined") {
        JobMenusBlip[uid].destroy();
        JobMenusBlip[uid] = undefined;
    }
    if (dir != undefined) {
        JobMenusBlip[uid] = mp.blips.new(type, position,
            {
                name: names,
                scale: 1,
                color: 4,
                alpha: 255,
                drawDistance: 100,
                shortRange: false,
                rotation: 0,
                dimension: 0
            });
    }

});
mp.events.add('deleteJobMenusBlip', function (uid) {
    if (typeof JobMenusBlip[uid] == "undefined") return;
    JobMenusBlip[uid].destroy();
    JobMenusBlip[uid] = undefined;
});


// Job Diver //
mp.events.add('OpenDiver', (money, level, currentjob, work) => {
    if (global.menuCheck()) return;
    jobs.execute(`Diver.set('${money}', '${level}', '${currentjob}', '${work}')`);
    jobs.execute('Diver.active=1');
    global.menuOpen();
});

mp.events.add('CloseDiver', () => {
    jobs.execute('Diver.active=0');
    global.menuClose();
});

mp.events.add("selectJobDiver", (jobid) => {
    if (new Date().getTime() - global.lastCheck < 1000) return;
    global.lastCheck = new Date().getTime();
    mp.events.callRemote("jobJoinDiver", jobid);
});

mp.events.add('secusejobDiver', (jobsid) => {
    jobs.execute(`Diver.setnewjob('${jobsid}')`);
});

mp.events.add('enterJobDiver', (work) => {
    mp.events.callRemote('enterJobDiver', work);
});

// Job StatsInfoDiver //
mp.events.add('JobStatsInfoDiver', (money, objects, obji) => {
    jobs.execute('JobStatsInfoDiver.active=1');
    jobs.execute(`JobStatsInfoDiver.set('${money}', '${objects}', '${obji}')`);
});
mp.events.add('CloseJobStatsInfoDiver', () => {
    jobs.execute('JobStatsInfoDiver.active=0');
});

// Возду для водолаза
var player = mp.players.local;
mp.events.add("startdiving", () => {
    player.setMaxTimeUnderwater(1000);
});
mp.events.add("stopdiving", () => {
    player.setMaxTimeUnderwater(10);
});

// Объекты для водолаза
var ObjectsJob = [];
mp.events.add("createObjectJobs", (uid, name, x, y, z) => {
    ObjectsJob[uid] = mp.game.object.createObject(mp.game.joaat(name), x, y, z, false, false, false);
});
mp.events.add("deleteObjectJobs", (uid) => {
    mp.game.object.deleteObject(ObjectsJob[uid]);
});