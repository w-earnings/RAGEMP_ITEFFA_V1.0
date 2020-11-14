var itemsData = {
    "-1": "Маска",
    "-3": "Перчатки",
    "-4": "Штаны",
    "-5": "Рюкзак",
    "-6": "Обувь",
    "-7": "Аксессуар",
    "-8": "Нижняя одежда",
    "-9": "Бронежилет",
    "-10": "Украшения",
    "-11": "Верхняя одежда",
    "-12": "Головной убор",
    "-13": "Очки",
    "-14": "Аксессуар",
    0: "Test Item",
    1: "Аптечка",
    2: "Канистра",
    3: "Чипсы",
    4: "Пиво",
    5: "Пицца",
    6: "Бургер",
    7: "Хот-Дог",
    8: "Сэндвич",
    9: "eCola",
    10: "Sprunk",
    11: "Отмычка для замков",
    12: "Сумка с деньгами",
    13: "Материалы",
    14: "Марихуана",
    15: "Сумка с дрелью",
    16: "Военная отмычка",
    17: "Мешок",
    18: "Стяжки",
    19: "Ключи от машины",
    40: "Подарок",
    41: "Связка ключей",

    20: `"На корке лимона"`,
    21: `"На бруснике"`,
    22: `"Русский стандарт"`,
    23: `"Asahi"`,
    24: `"Midori"`,
    25: `"Yamazaki"`,
    26: `"Martini Asti"`,
    27: `"Sambuca"`,
    28: `"Campari"`,
    29: `"Дживан"`,
    30: `"Арарат"`,
    31: `"Noyan Tapan"`,

    100: "Pistol",
    101: "Combat Pistol",
    102: "Pistol .50",
    103: "SNS Pistol",
    104: "Heavy Pistol",
    105: "Vintage Pistol",
    106: "Marksman Pistol",
    107: "Revolver",
    108: "AP Pistol",
    109: "Stun Gun",
    110: "Flare Gun",
    111: "Double Action",
    112: "Pistol Mk2",
    113: "SNSPistol Mk2",
    114: "Revolver Mk2",

    115: "Micro SMG",
    116: "Machine Pistol",
    117: "SMG",
    118: "Assault SMG",
    119: "Combat PDW",
    120: "MG",
    121: "Combat MG",
    122: "Gusenberg",
    123: "Mini SMG",
    124: "SMG Mk2",
    125: "Combat MG Mk2",

    126: "Assault Rifle",
    127: "Carbine Rifle",
    128: "Advanced Rifle",
    129: "Special Carbine",
    130: "Bullpup Rifle",
    131: "Compact Rifle",
    132: "Assault Rifle Mk2",
    133: "Carbine Rifle Mk2",
    134: "Special Carbine Mk2",
    135: "Bullpup Rifle Mk2",

    136: "Sniper Rifle",
    137: "Heavy Sniper",
    138: "Marksman Rifle",
    139: "Heavy Sniper Mk2",
    140: "Marksman Rifle Mk2",

    141: "Pump Shotgun",
    142: "SawnOff Shotgun",
    143: "Bullpup Shotgun",
    144: "Assault Shotgun",
    145: "Musket",
    146: "Heavy Shotgun",
    147: "Double Barrel Shotgun",
    148: "Sweeper Shotgun",
    149: "Pump Shotgun Mk2",

    180: "Нож",
    181: "Дубинка",
    182: "Молоток",
    183: "Бита",
    184: "Лом",
    185: "Гольф клюшка",
    186: "Бутылка",
    187: "Кинжал",
    188: "Топор",
    189: "Кастет",
    190: "Мачете",
    191: "Фонарик",
    192: "Швейцарский нож",
    193: "Кий",
    194: "Ключ",
    195: "Боевой топор",

    200: "Пистолетный калибр",
    201: "Малый калибр",
    202: "Автоматный калибр",
    203: "Снайперский калибр",
    204: "Дробь",
	
	// Fishing
	205: "Удочка",
	206: "Улучшенная удочка",
	207: "Удочка MK2",
    208: "Наживка",
    209: "Корюшка",
    210: "Кунджа",
    211: "Лосось",
    212: "Окунь",
    213: "Осётр",
    214: "Скат",
	215: "Тунец",
	216: "Угорь",
	217: "Чёрный амур",
	218: "Щука",
	
	// AlcoShop
	219: "Martini Asti",
	220: "Sambuca",
	221: "Водка с лимоном",
	222: "Водка на бруснике",
	223: "Русский стандарт",
	224: "Коньяк Дживан",
	225: "Коньяк Арарат",
	226: "Пиво разливное",
	227: "Пиво бутылочное",
	228: "Кальян",
	
	// Repair
	250: "Рем. Комплект",
	
	251: "Бинт",
	252: "Аптечка", 
	253: "Таблетки",
	254: "Шприц адреналина",
	
	//Farm
	255: "Листья коки",
	
	//Diver
	256: "Золото",
	257: "Железяка"
}

var itemsInfo = {
	"-1": "Маска - поможет скрыть твою личность.",
    "-3": "Перчатки.",
    "-4": "Штаны.",
    "-5": "Рюкзак.",
    "-6": "Обувь.",
    "-7": "Украшения/Галстук.",
    "-8": "Нижния одежда.",
    "-9": "Бронежилет - поможет получать меньже урона от выстрела.",
    "-10": ".",
    "-11": "Верхняя одежда.",
    "-12": "Головные уборы.",
    "-13": "Очки.",
    "-14": "Часы/Брелки на руку.",
    1:	"Аптечка - поможет реанимировать или восстановить себе здоровье.",
    2: "Конистра бензина - поможет заправить ТС в любом месте.",
    3: "Пачка чипсов - поможет восстановить голод.",
    4:"Без алкольное пиво - дописать.",
    5:"Пицца - поможет восстановить голод.",
    6: "Бургер - поможет восстановить голод.",
    7:  "Хот-Дог - поможет восстановить голод.",
	8:	"Сэндвич - поможет восстановить голод.",
	9:	"Cola - поможет восстановить жажду.",
	10:	"Sprunk - поможет восстановить жажду.",
	11:	"Отмычка - поможет вскрыть замок.",
	12:	"Сумка с деньгами - скорее всего деньги ворованые.",
	13:	"Материалы - помогут скрафтить оружие.",
	14:	"Пакетик марихуаны - поможет восстановить здоровье.",
	15:	"Сумка с дрелью - поможет вскрыть хранилище.",
	16:	"Военная отмычка - поможет скрыть военую технику.",
	17:	"Мешок - надев его на человека он не будет видеть.",
	18:	"Стяжки - помогут связать человека.",
	19:	"Ключи - помогут открыть ваше личное ТС.",
	20:	"Выпивка - даёт алкогольный эффект.",
	21:	"Выпивка - даёт алкогольный эффект.",
	22:	"Выпивка - даёт алкогольный эффект.",
	23:	"Выпивка - даёт алкогольный эффект.",
	24:	"Выпивка - даёт алкогольный эффект.",
	25:	"Выпивка - даёт алкогольный эффект.",
	26:	"Выпивка - даёт алкогольный эффект.",
	27:	"Выпивка - даёт алкогольный эффект.",
	28:	"Выпивка - даёт алкогольный эффект.",
	1:	"Аптечка - поможет реанимировать или восстановить себе здоровье.",
}

Vue.component('item', {
	template: '<div :class="test"><div class="item" v-bind:title="name" v-bind:weight="(weight*count).toFixed(2)" :fastslot="fast_slot" v-bind:class="{active: isactive}" @click.right.prevent="select"> \
    <img :src="src"><span>{{count}}</span><p class="sub">{{subdata}}</p><p class="names">{{name}}<br><a>{{info}}</a><b>{{count}} шт.</b></p></div></div>',
    props: ['id', 'index', 'count', 'isactive', 'type', 'subdata'],
    data: function () {
        return {
            src: 'items/' + this.id + '.png',
			title: itemsData[this.id],
            name: itemsData[this.id],
            info: itemsInfo[this.id],
			test: 'item' + this.id + 'ma',
        }
    },
    methods: {
        select: function (event) {
            board.sType = (this.type == 'inv') ? 1 : 0;
            board.sID = this.id;
            board.sIndex = this.index;
            context.type = (this.type == 'inv') ? 1 : 0;
			context.fastSlot = this.fast_slot
        }
    }
})
var board = new Vue({
    el: ".board",
    data: {
        active: false,
        outside: false,
		text: ["Уровень", "Предупреждения", "Дата создания", "Номер телефона", "Номер счёта", "Номер паспорта", "Организация", "Ранг", "Работа", "Статус"],
		outType: 0,
        outHead: "Внешний", 
		//		0        1  	 2 		     3 		        4	   5            6		  7			  8				9	  10	  11        12	      13    
		stats: ["15", "30/60", "777 777", "Администрация", "2", "A B C D", "01.05.1980", "Строитель", "CityHall", "17", "Vovan", "Putin", "333 666", "4276 7700"],
        items: [[-6, 5, 1],[-7, 5, 1],[-8, 5, 1],[-9, 5, 1],[-11, 5, 1],[-12, 5, 1],[-13, 5, 1],[-14, 5, 1],[-1, 5, 1],[-3, 5, 1],[-4, 5, 1],[1, 5, 1],[1, 5, 1],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[1, 5, 1],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0]],
        outitems: [[1, 5, 1],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0]],
		money: 0,
		donate: 0,
		bank: 0, 
		page: 1,
        sIndex: 0,
        sType: 0,
        sID: 0,
        key: 0,
		arraymax: 0
    },
    methods: {
        context: function (event) {
            if (clickInsideElement(event, 'item')) {
                context.show(event.pageX, event.pageY)
            } else {
                context.hide()
            }
        },
        hide: function (event) {
            context.hide()
        },
        outSet: function (json) {
            this.key++
            this.outType = json[0]
            this.outHead = json[1]
            this.outitems = json[2]
        },
		pages: function(id){
            this.page = id;
        },
		itemsSet: function(t) {
                this.key++, this.items = t, this.usedFastSlots = [!1, !1, !1, !1, !1];
                for (let t = 1; t < 6; t++) mp.trigger("bindSlotKey", 0, t, !1);
                for (let t = 0; t < this.items.length; t++) {
                    const s = this.items[t];
                    s[6] > 0 && (this.usedFastSlots[s[6]] = !0, mp.trigger("bindSlotKey", t, s[6], !0))
                }
                this.updateWeight()
            },
            itemUpd: function(t, s) {
                this.key++, this.items[t] = s, this.updateWeight()
            }, 
			updateWeight: function() {
                let t = 0;
                this.items.forEach(s => {
                    t += s[4] * s[1]
                }), this.weight = t
            },
            useFastSlot: function(t) {
                this.usedFastSlots[t] || (this.selectFastSlot = !1, this.sFastSlot = t, this.items[board.sIndex][6] = this.sFastSlot, this.usedFastSlots[t] = !0, mp.trigger("useFastSlot", this.sIndex, this.sFastSlot, 0))
            },
            unsetFastSlot: function() {
                let t = this.items[board.sIndex][6];
                this.usedFastSlots[t] = !1, this.items[board.sIndex][6] = 0, this.key++, mp.trigger("useFastSlot", this.sIndex, 0, t)
            },
        send: function (id) {
            let type = (this.sType) ? 0 : this.outType
            mp.trigger('boardCB', id, type, this.sIndex)
        }
    }
})
var context = new Vue({
    el: ".context_menu",
    data: {
		men: ["Использовать", "Передать", "Взять", "Выбросить"],
        active: false,
        style: '',
        type: true,
        fastSlot: -1
    },
    methods: {
        show: function (x, y) {
            this.style = `left:${x}px;top:${y}px;`
            this.active = true
        },
        hide: function () {
            this.active = false
        },
        btn: function (id) {
            this.hide()
            board.send(id)
        },
        setFastSlot: function() {
		board.usedFastSlots.includes(!1) && (this.hide(), board.selectFastSlot = !0)
            },
            unsetFastSlot: function() {
                this.hide(), board.unsetFastSlot()
            }
    }
})
function clickInsideElement(e, className) {
    var el = e.srcElement || e.target;
    if (el.classList.contains(className)) {
        return el;
    } else {
        while (el = el.parentNode) {
            if (el.classList && el.classList.contains(className)) {
                return el;
            }
        }
    }
    return false;
}