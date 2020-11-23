const hclass = (id, name, title, room) => ({
  id,
  name,
  title,
  room,
});//2AC
const houseClasses = [
  hclass(0, "Трейлер", 0),
  hclass(1, "Econom", "Нищебродские дома", 1),
  hclass(2, "Ranch+", "Ну, так себе", 2),
  hclass(3, "Premium", "Уже неплохо", 4),
  hclass(4, "Cottage", "А ты двигаешься", 5),
  hclass(5, "Mansion", "Ого, ничего себе", 6),
  hclass(6, "Villa", "Роскошная вилла, а ты богач", 8),
]

var realtorMenu = new Vue({
  el: '#app',
  data: {
    active: false,
    classes: houseClasses,
    houses: [],
    street: null,
    crossingRoad: null,
    pageNum: 0,
    selectedClass: null,
    activeClassModal: false,
    modalVisible: false,
    modalFilter: false,
    hclass: null,
    house: null,
    stars: 3,
    maxPrice: null,
    minPrice: null,
    search: '',
    filterPrice(house) {
      if(this.minPrice == null && this.maxPrice == null || this.maxPrice == "" && this.minPrice == "") return true;
      //console.log(house[2]);
	  var maxI;
	  if(this.maxPrice == "" || this.maxPrice == null) {
		  maxI = 100000000000000000000
	  } else maxI = this.maxPrice;
      var max = house[2] <= maxI ? true : false;
      if(this.minPrice <= house[2] && max) return true;
      else return false;
    }
  },
  methods: {
    selectClass: function(index) {
      this.selectedClass = index
      this.pageNum = 1
      mp.trigger("SelectHouseClass", index)
    },
    classInfo(hclass) {
      this.hclass = hclass
      this.activeClassModal = true
    },
    showModalFitler() {
      this.modalFilter = !this.modalFilter
    },
    info(house) {
      this.house = house
      mp.trigger("getStreetAndAreaHouse", this.house[3].x, this.house[3].y, this.house[3].z)
      this.modalVisible = true
    },
    setGPS() {
      mp.trigger("buyInfoHome", this.selectedClass , this.house[3].x, this.house[3].y)
      this.resetData()
      this.houses = null
      this.active = false
      //console.log("SUCCESS //1AL")
    },
    cancelmodal() {
      this.activeClassModal = false
      this.modalVisible = false
      this.hclass = null
      this.house = null
      //console.log("CANCEL //1AL")
    },
    cancel: function() {
      this.pageNum == 0 ? (mp.trigger("closeRealtorMenu"), this.active = false) : this.resetData();
    },
    getCountStar(index, count) {
      console.log(index, count)
      switch (index) {
        case 0:
          if(count == 1) return "fa-star-half-o";
          else return "fa-star";
        case 1:
          if(count < 3) return "fa-star-o";
          if(count == 3) return "fa-star-half-o";
          else return "fa-star";
          break;
        case 2:
          if(count >= 6) return "fa-star";
          if(count == 5) return "fa-star-half-o";
          else return "fa-star-o";
          break;
      }
    },
    resetData() {
      this.selectedClass = null
      this.houses = []
      this.house = null
      this.pageNum = 0
      this.modalFilter = false
      this.modalVisible = false
      this.activeClassModal = false
	  this.search = ''
	  this.minPrice = null
	  this.maxPrice = null
    },
  },
  computed: {
    filterHouse() {
      return this.houses.filter(house => {
        return house[0].toString().indexOf(this.search) > -1
      })
    },
  }
})
