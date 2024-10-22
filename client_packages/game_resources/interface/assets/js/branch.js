var branch = new Vue({
    el: '.branch',
    data: {
      active: false,
      type: 1,
      page: 0,
      number: "545 548",
      holder: "Project iTeffa",
      balance: "800,000",
      money: "800,000",
      taxacc: "154,333",
      subdata: "154/333",
      plholder: "Сумма",
      value: "",
    },
    methods: {
      set: function (num, name, bal, tax, sub) {
        this.number = num;
        this.holder = name;
        this.balance = bal;
        this.taxacc = tax;
      },
      open: function (json) {
        this.reset();
        this.plholder = json[2];
        this.subdata = json[1];
        this.type = json[0];
      },
      btn: function (e) {
        let id = Number(e.target.id)
        mp.trigger("branchCB", this.type, id);
      },
      next: function () {
        mp.trigger('branchVal', this.value);
      },
      exit: function () {
        mp.trigger('closebranch');
        this.page = 0;
      },
      prev: function () {
        mp.trigger('branchCB', this.type, 0);
      },
      pages: function (id) {
        this.page = id;
        this.type = 1;
  
        function animate() {
          if (TWEEN.update()) {
            requestAnimationFrame(animate)
          }
        }
      },
      reset: function () {
        this.subdata = null;
        this.value = "";
        this.type = 1;
      }
    }
  })