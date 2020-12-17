var containerMenu = new Vue({
    el: '#app',
    data: {
      active: false,
      page: 0,
      container: [],
      //TX1
    },
    methods: {
      setinfo(json) {
        this.container = json
        this.active = true
      },
      changePage(page){
        this.page = page
      },
      openContainer(){
        mp.trigger("openContainer");
        this.closeMenu()
      },
      closeMenu(){
        this.page = 0
        this.active = false
        mp.trigger("closeContainer");
        //TX1
      }
    },
    computed: {
        filterLoots() {
          return [...new Set(this.container.Loots)];
        }
    }
  })
  