var admlist = new Vue({
  el: "#app",
  data: {
    active: false,
    activeModal: false,
    page: null,
    items: [],
    item: [],
    player: {},
    cmdlist: [],
    text: [],
    getArgs: function(command) {
      return cmds[command] && cmds[command].args || "нет аргументов";
	  //9IP
    },
    getState: function(command) {
      return cmds[command] && cmds[command].target || false;
    },
    filterFontSize(length) {
      if(length < 11) return "";
      if (length >= 11 && length <= 14) return `font-size:0.79em`;
      if (length >= 15) return `font-size: `+ (0.9 - (length/100 * 1.5)) +`em`;
    },
  },
  methods: {
    reset: function() {
      this.items = [];
      this.item = [];
      this.active = false;
    },
    closepanel: function() {
      this.reset();
      mp.trigger("closeAdminPanel");
	  //9IP
    },
    selectPlayer(item) {
      this.page = "cmds";
      this.activeModal = true;
      this.item = item;
    },
    getPlayerInfo(item) {
      this.page = "info";
      this.activeModal = true;
      this.item = item;
      mp.trigger("getPlayerInfo", item[1]);
    },
    cancelmodal() {
      this.activeModal = false;
      this.item = [];
      this.text = [];
      this.page = null;
    },
    comand(cmd) {
      this.text = [];
	  //9IP
      mp.invoke("command", cmd);
    },
  }
})
