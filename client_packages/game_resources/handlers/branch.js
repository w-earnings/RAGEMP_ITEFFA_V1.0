global.branch = mp.browsers.new('package://game_resources/interface/branch.html');

var branchIndex = 0;
mp.events.add('openbranch', () => {
  if (global.menuCheck()) return;
  branch.execute('branch.active=1');
  menuOpen();
});
mp.events.add('closebranch', () => {
  menuClose();
  branch.execute('branch.reset();branch.active=0');
})
mp.events.add('setbranch', (num, name, bal, sub) => {
  branch.execute(`branch.set('${num}','${name}','${bal}','${sub}')`);
})
mp.events.add('setbank', (bal) => {
  branch.execute(`branch.balance="${bal}"`);
})
mp.events.add('branchCB', (type, data) => {
  mp.events.callRemote('branchCB', type, data);
})
branchTcheck = 0;
mp.events.add('branchVal', (data) => {
  if (new Date().getTime() - branchTcheck < 1000) {
    mp.events.callRemote('branchDP');
  } else {
    mp.events.callRemote('branchVal', data);
    branchTcheck = new Date().getTime();
  }
})
mp.events.add('branchOpen', (data) => {
  branch.execute(`branch.open(${data})`);
})
mp.events.add('branchOpenBiz', (data1, data2) => {
  branch.execute(`branch.open([3, ${data1}, ${data2}])`);
})
mp.events.add('branch', (index, data) => {
  if (index == 4) {
    BRANCHTemp = data;
    branch.execute('branch.change(44)');
  } else if (index == 44) {
    mp.events.callRemote('branch', 4, data, BRANCHTemp);
    branch.execute('branch.reset()');
    return;
  } else if (index == 33) {
    mp.events.callRemote('branch', 3, data, BRANCHTemp);
  } else {
    mp.events.callRemote('branch', index, data);
    branch.execute('branch.reset()');
  }
})
let BRANCHTemp = "";