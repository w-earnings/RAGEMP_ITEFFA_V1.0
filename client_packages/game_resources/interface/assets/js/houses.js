var hmBuy = new Vue({
    el: ".hmBuy",
    data: {
      active: false,
      header: "Информация о доме",
      menu: 0,
      id: 0,
      owner: "",
      type: "",
      locked: ["Открыты", "Закрыты"],
      price: 0,
      garage: 5,
      roommates: 0,
    },
    methods: {
      set: function (id, Owner, Type, Locked, Price, Garage, Roommates) {
        this.id = id;
        this.owner = Owner;
        this.type = Type;
        this.locked = Locked;
        this.price = Price;
        this.garage = Garage;
        this.roommates = Roommates;
        this.menu = id;
      },
      exit: function () {
        mp.trigger('CloseHouseMenuBuy');
        this.menu = 0;
      },
      buy: function (id) {
        mp.trigger("buyHouseMenu", id);
        this.menu = 0;
      },
      int: function (id) {
        mp.trigger("Interior", id);
        this.menu = 0;
      },
      open: function (id) {
        this.menu = id;
      }
    }
  });
  var Order = new Vue({
    el: ".Order",
    data: {
      active: false,
      menu: 0,
      indexM: 0,
      indexC: 0,
      ordersIDs: [3231321221, 199, 1999, 1999, 1999, 1999, 1999, 1999, 1999, 1999, 1999],
      OrderNames: ["Tesla Model S", "Tesla Model 3", "Tesla Model X", "Tesla Model X", "Tesla Model X", "Tesla Model X", "Tesla Model X", "Tesla Model X",
        "Tesla Model X", "Tesla Model X", "Tesla Model X", "Tesla Model X", "Tesla Model X"
      ],
      OrderPrices: [3231321221, 199, 1999],
    },
    methods: {
      set: function (ordersIDs, OrderNames, OrderPrices) {
        this.ordersIDs = ordersIDs.split(',');
        this.OrderNames = OrderNames.split(',');
        this.OrderPrices = OrderPrices.split(',');
        this.menu = id;
      },
      exit: function () {
        mp.trigger('CloseOrder');
        this.menu = 0;
      },
      take: function (uid) {
        console.log('take')
        mp.trigger("OrderTake", uid);
      },
      open: function (id) {
        this.menu = id;
      },
      reset: function () {
        this.OrderPrices = -1
        this.indexM = 0
        this.indexC = 0
        this.OrderNames = []
        this.ordersIDs = []
        this.OrderPrices = []
      }
    }
  });
  var hm = new Vue({
    el: ".hm",
    data: {
      active: false,
      header: "Информация о доме",
      id: 0,
      owner: "Kirill Efmatov",
      type: "Premium+",
      locked: false + ["Закрыты"],
      price: 150000,
      garage: 5,
      roommates: 0,
    },
    methods: {
      set: function (id, Owner, Type, Locked, Price, Garage, Roommates) {
        this.id = id;
        this.owner = Owner;
        this.type = Type;
        this.locked = Locked;
        this.price = Price;
        this.garage = Garage;
        this.roommates = Roommates;
      },
      exit: function () {
        mp.trigger('CloseHouseMenu');
      },
      select: function (id) {
        mp.trigger("selectSchool", id);
      },
      enter: function (id) {
        mp.trigger("GoHouseMenu", id);
      },
      lock: function (id) {
        mp.trigger("LockedHouse", id);
      },
      sell: function (id) {
        mp.trigger("SellHome", id);
      },
      warn: function (id) {
        mp.trigger("WarnHouse", id);
      },
      reset: function () {
        this.locked = 0
        this.locked = ["Открыты", "Закрыты"]
      }
    }
  });
  var hmExit = new Vue({
    el: ".hmExit",
    data: {
      active: false,
      header: "Выход",
    },
    methods: {
      exit: function () {
        mp.trigger('exitHouse');
      },
      garage: function (id) {
        mp.trigger("Garage", id);
        this.menu = 0;
      },
      close: function () {
        mp.trigger('CloseExitHouseMenu');
      }
    }
  });

  var MyyHouseMenu = new Vue({
    el: ".MyyHouseMenu",
    data: {
      active: false,
      header: "Управление домом",
      id: 0,
      owner: "Петя",
      type: 0,
      price: 0,
      garage: 0,
      roommates: 0,
      infoM: 1,
      carM: 0,
      furnitureM: 0,
      roommatesM: 0,
    },
    methods: {
      info: function (id, Owner, Type, Price, Garage, Roommates) {
        this.id = id;
        this.owner = Owner;
        this.type = Type;
        this.price = Price;
        this.garage = Garage;
        this.roommates = Roommates;
      },
      exit: function () {
        this.active = false;
        mp.trigger('CloseMyyHouseMenu');
      },
      infoMm: function (id) {
        this.infoM = id;
        this.carM = 0;
        this.furnitureM = 0;
        this.roommatesM = 0;
      },
      carMm: function (id) {
        this.infoM = 0;
        this.carM = id;
        this.furnitureM = 0;
        this.roommatesM = 0;
      },
      furnitureMm: function (id) {
        this.infoM = 0;
        this.carM = 0;
        this.furnitureM = id;
        this.roommatesM = 0;
      },
      roommatesMm: function (id) {
        this.infoM = 0;
        this.carM = 0;
        this.furnitureM = 0;
        this.roommatesM = id;
      }
    }
  });