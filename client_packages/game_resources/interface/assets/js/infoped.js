var infoped = new Vue({
    el: ".infoped",
    data: {
        active: false,
		menu: 0,
    },
    methods: {
        exit: function () {
            mp.trigger('CloseInfoMenu');
			this.menu = 0;
        },
        open: function(id){
            this.menu = id;
        }
    }
});