var HUD = new Vue({
  el: ".inGameHud",
  data: {
    show: false,
    bonusblock: true,
    lastbonus: null,
    ammo: 0,
    money: 0,
    bank: 0,
    mic: false,
    time: "00:00",
    date: "00.00.00",
    street: "iTeffa.com",
    crossingRoad: "IT-Studio",
    server: 0,
    playerId: 0,
    online: 0,
    inVeh: false,
    belt: false,
    engine: false,
    doors: false,
    speed: 0,
    rpm: 0,
    gear: 0,
    cruiseColor: '#ffffff',
    ifuel: 0,
    fuel: 0,
    hp: 0,
  },
  methods: {
    setTime: (time, date) => {
      this.time = time;
      this.date = date;
    },
    showbonus() {
      this.bonusblock = !this.bonusblock;
    },
    updateSpeed(currentspeed, maxspeed = 350) {
      this.speed = currentspeed;
      const meters = document.querySelectorAll('svg[data-value] .meter');
      meters.forEach((path) => {
        let length = path.getTotalLength();
        let c = parseInt(path.parentNode.getAttribute('data-value'));
        let value = 0.4166666666666667 * c;
        let rotate = -130 + (value * 2.61153846153846);
        let to = length * ((100 - value) / 100);
        path.getBoundingClientRect();
        path.style.strokeDashoffset = Math.max(0, to);
      });
    }
  }
});