var melogMarket = new Vue({
  el: '#app',
  data: {
    active: false,
    header: "Скупщик - Мелог",
    page: 0,
    onWork: false,
    buyitems: [],
    sellitems: [],
    curs: 0,
    hays: 0,
    seedcount: 0,
	buyValue: 1,
	sellValue: 1,
  },
  methods: {
    setinfo(json) {
      this.curs = json[0]
      this.hays = json[1]
      this.seedcount = json[2]
    },
    changePage(value) {
      this.page = value
      mp.trigger("changePage", value);
    },
	buy(item) {
		this.buyValue = parseInt(this.buyValue);
		if(this.buyValue <= 0 || this.buyValue == null) this.buyValue = 1;
		mp.trigger("farmerBuy", item, this.buyValue);
	},
	sell(item) {
		this.sellValue = parseInt(this.sellValue)
		if(this.sellValue <= 0 || this.sellValue == null) this.sellValue = 1;
		mp.trigger("farmerSell", item, this.sellValue);
	},
    closeMenu() {
      this.active = false
	  this.page = 0
      mp.trigger("closeMarketMenu")
    }
  }
})
