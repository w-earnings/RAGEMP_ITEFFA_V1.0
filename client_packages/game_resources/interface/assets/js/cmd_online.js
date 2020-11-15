var admlist = new Vue({
    el: "#app",
    data: {
        active: false,
        activeModal: false,
        items: [],
        item: [],
        cmdlist: [],
        text: [],
        getArgs: function (command) {
            return cmds[command] && cmds[command].args || "нет аргументов";
        },
		getState: function (command) {
            return cmds[command] && cmds[command].target || false;
        },
    },
    methods: {
        reset: function () {
            this.items = [];
            this.item = [];
            this.online = 0;
        },
        closepanel: function () {
            this.reset();
            this.active = false;
			mp.trigger("closeCmdOnline");
        },
        selectPlayer(item) {
          this.activeModal = true;
          this.item = item;
        },
        cancelmodal() {
          this.activeModal = false;
          this.item = [];
          this.text = []
        },
        comand(cmd) {
          this.text = []
		  mp.invoke("command", cmd);
        },
    }
})
