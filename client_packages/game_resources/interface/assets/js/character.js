var data = {
    "test": ["test1", "test2", "test3", "test4", ],
    "father": ["Benjamin", "Daniel", "Joshua", "Noah", "Andrew", "Juan", "Alex", "Isaac", "Evan", "Ethan", "Vincent", "Angel", "Diego", "Adrian", "Gabriel",
      "Michael", "Santiago", "Kevin", "Louis", "Samuel", "Anthony", "Claude", "Niko", "John"
    ],
    "mother": ["Hannah", "Aubrey", "Jasmine", "Gisele", "Amelia", "Isabella", "Zoe", "Ava", "Camila", "Violet", "Sophia", "Evelyn", "Nicole", "Ashley",
      "Gracie", "Brianna", "Natalie", "Olivia", "Elizabeth", "Charlotte", "Emma", "Misty"
    ],
    "eyebrowsM": ["None", "Balanced", "Fashion", "Cleopatra", "Quizzical", "Femme", "Seductive", "Pinched", "Chola", "Triomphe", "Carefree", "Curvaceous",
      "Rodent", "Double Tram", "Thin", "Penciled", "Mother Plucker", "Straight and Narrow", "Natural", "Fuzzy", "Unkempt", "Caterpillar", "Regular",
      "Mediterranean", "Groomed", "Bushels", "Feathered", "Prickly", "Monobrow", "Winged", "Triple Tram", "Arched Tram", "Cutouts", "Fade Away", "Solo Tram"
    ],
    "eyebrowsF": ["None", "Balanced", "Fashion", "Cleopatra", "Quizzical", "Femme", "Seductive", "Pinched", "Chola", "Triomphe", "Carefree", "Curvaceous",
      "Rodent", "Double Tram", "Thin", "Penciled", "Mother Plucker", "Straight and Narrow", "Natural", "Fuzzy", "Unkempt", "Caterpillar", "Regular",
      "Mediterranean", "Groomed", "Bushels", "Feathered", "Prickly", "Monobrow", "Winged", "Triple Tram", "Arched Tram", "Cutouts", "Fade Away", "Solo Tram"
    ],
    "beard": ["None", "Light Stubble", "Balbo", "Circle Beard", "Goatee", "Chin", "Chin Fuzz", "Pencil Chin Strap", "Scruffy", "Musketeer", "Mustache",
      "Trimmed Beard", "Stubble", "Thin Circle Beard", "Horseshoe", "Pencil and 'Chops", "Chin Strap Beard", "Balbo and Sideburns", "Mutton Chops",
      "Scruffy Beard", "Curly", "Curly & Deep Stranger", "Handlebar", "Faustic", "Otto & Patch", "Otto & Full Stranger", "Light Franz", "The Hampstead",
      "The Ambrose", "Lincoln Curtain"
    ],
    "hairM": [
      "Лысый", "Прическа №1", "Прическа №2", "Прическа №3", "Прическа №4", "Прическа №5", "Прическа №6", "Прическа №7", "Прическа №8", "Прическа №9",
      "Прическа №10", "Прическа №11", "Прическа №12", "Прическа №13", "Прическа №14", "Прическа №15", "Прическа №16", "Прическа №17", "Прическа №18",
      "Прическа №19", "Прическа №20", "Прическа №21", "Прическа №22", "Прическа №24", "Прическа №25", "Прическа №26", "Прическа №57", "Прическа №28",
      "Прическа №29", "Прическа №30", "Прическа №31", "Прическа №32", "Прическа №33", "Прическа №34", "Прическа №35", "Прическа №36", "Прическа №37",
      "Прическа №38", "Прическа №39", "Прическа №40", "Прическа №41", "Прическа №42", "Прическа №43", "Прическа №44", "Прическа №45", "Прическа №46",
      "Прическа №47", "Прическа №48", "Прическа №49", "Прическа №50", "Прическа №51", "Прическа №52", "Прическа №53", "Прическа №54", "Прическа №55",
      "Прическа №56", "Прическа №57", "Прическа №58", "Прическа №59", "Прическа №60", "Прическа №61", "Прическа №62", "Прическа №63", "Прическа №64",
      "Прическа №65", "Прическа №66", "Прическа №67", "Прическа №68", "Прическа №69", "Прическа №70", "Прическа №71", "Прическа №72", "Прическа №73",
      "Прическа №74", "Прическа №75", "Прическа №76", "Прическа №77", "Прическа №78", "Прическа №79", "Прическа №80", "Прическа №81", "Прическа №82",
      "Прическа №83", "Прическа №84", "Прическа №85", "Прическа №86", "Прическа №87", "Прическа №88", "Прическа №89", "Прическа №90", "Прическа №91",
      "Прическа №92", "Прическа №93", "Прическа №94", "Прическа №95", "Прическа №96", "Прическа №97", "Прическа №98", "Прическа №99", "Прическа №100",
      "Прическа №101", "Прическа №102", "Прическа №103", "Прическа №104", "Прическа №105", "Прическа №106", "Прическа №107", "Прическа №108", "Прическа №109",
      "Прическа №110", "Прическа №111"
    ],
    "hairF": [
      "Лысая", "Прическа №1", "Прическа №2", "Прическа №3", "Прическа №4", "Прическа №5", "Прическа №6", "Прическа №7", "Прическа №8", "Прическа №9",
      "Прическа №10", "Прическа №11", "Прическа №12", "Прическа №13", "Прическа №14", "Прическа №15", "Прическа №16", "Прическа №17", "Прическа №18",
      "Прическа №19", "Прическа №20", "Прическа №21", "Прическа №22", "Прическа №24", "Прическа №25", "Прическа №26", "Прическа №57", "Прическа №28",
      "Прическа №29", "Прическа №30", "Прическа №31", "Прическа №32", "Прическа №33", "Прическа №34", "Прическа №35", "Прическа №36", "Прическа №37",
      "Прическа №38", "Прическа №39", "Прическа №40", "Прическа №41", "Прическа №42", "Прическа №43", "Прическа №44", "Прическа №45", "Прическа №46",
      "Прическа №47", "Прическа №48", "Прическа №49", "Прическа №50", "Прическа №51", "Прическа №52", "Прическа №53", "Прическа №54", "Прическа №55",
      "Прическа №56", "Прическа №57", "Прическа №58", "Прическа №59", "Прическа №60", "Прическа №61", "Прическа №62", "Прическа №63", "Прическа №64",
      "Прическа №65", "Прическа №66", "Прическа №67", "Прическа №68", "Прическа №69", "Прическа №70", "Прическа №71", "Прическа №72", "Прическа №73",
      "Прическа №74", "Прическа №75", "Прическа №76", "Прическа №77", "Прическа №78", "Прическа №79", "Прическа №80", "Прическа №81", "Прическа №82",
      "Прическа №83", "Прическа №84", "Прическа №85", "Прическа №86", "Прическа №87", "Прическа №88", "Прическа №89", "Прическа №90", "Прическа №91",
      "Прическа №92", "Прическа №93", "Прическа №94", "Прическа №95", "Прическа №96", "Прическа №97", "Прическа №98", "Прическа №99", "Прическа №100",
      "Прическа №101", "Прическа №102", "Прическа №103", "Прическа №104", "Прическа №105", "Прическа №106", "Прическа №107", "Прическа №108", "Прическа №109",
      "Прическа №110", "Прическа №111", "Прическа №112", "Прическа №113", "Прическа №114", "Прическа №115", "Прическа №116", "Прическа №117"
    ],
    "hairColor": ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20"],
    "eyeColor": ["Green", "Emerald", "Light Blue", "Ocean Blue", "Light Brown", "Dark Brown", "Hazel", "Dark Gray", "Light Gray", "Pink", "Yellow", "Purple"]
  };
  Vue.component('list', {
    template: '<div v-bind:id="id" class="list">\
      <i @click="left" class="left flaticon-left-arrow"></i>\
      <div>{{ values[index] }}</div>\
      <i @click="right" class="right flaticon-arrowhead-pointing-to-the-right"></i></div>',
    props: ['id', 'num'],
    data: function () {
      return {
        index: 0,
        values: this.num ? [-1, -0.1, -0.2, -0.3, -0.4, -0.5, -0.6, -0.7, -0.8, -0.9, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1] : data[this.id],
      }
    },
    methods: {
      left: function (event) {
        this.index--
        if (this.index < 0) this.index = 0
        this.send()
      },
      right: function (event) {
        this.index++
        if (this.index == this.values.length) this.index = 0
        this.send()
      },
      send: function () {
        var value = this.num ? this.values[this.index] : this.index
        mp.trigger('editorList', this.id, Number(value))
      }
    }
  })
  var editor = new Vue({
    el: ".editor",
    data: {
      active: true,
      gender: true,
      isSurgery: false,
    },
    methods: {
      genderSw: function (type) {
        if (type) {
          this.gender = true
          mp.trigger('characterGender', "Male")
        } else {
          this.gender = false
          mp.trigger('characterGender', "Female")
        }
      },
      save: function () {
        mp.trigger('characterSave')
      }
    }
  });
  $(function () {
    $(document).on('input', 'input[type="range"]', function (e) {
      let id = e.target.id;
      let val = e.target.value;
      $('output#' + id).html(val);
      mp.trigger('editorList', id, Number(val));
    });
    $('input[type=range]').rangeslider({
      polyfill: false,
    });
    $('#gendermale').on('click', function () {
      $('#genderfemale').removeClass('on');
      $('#gendermale').addClass('on');
      editor.genderSw(true);
    });
    $('#genderfemale').on('click', function () {
      $('#gendermale').removeClass('on');
      $('#genderfemale').addClass('on');
      editor.genderSw(false);
    });
  });