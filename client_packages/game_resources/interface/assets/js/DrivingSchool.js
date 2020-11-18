

var DrivingSchool = new Vue({
    el: ".DrivingSchool",
    data: {
        active: false,
        header: "Автошкола",
        m0: 0, m1: 1, m2: 2, m3: 3, m4: 4, m5: 6, str: 0,
    },
    methods: {
        set: function (m0, m1, m2, m3, m4, m5) {
            this.money0 = m0;
            this.money1 = m1;
            this.money2 = m2;
            this.money3 = m3;
            this.money4 = m4;
            this.money5 = m5;
        },
        exit: function () {
            this.active = false;
            this.str = 0;
            mp.trigger('CloseDrivingSchool');
        },
        select: function (id) {
            mp.trigger("selectSchool", id);
        },
        page: function (page) {
            this.str = page;
        }

    }
});

var StatsDrivingSchool = new Vue({
    el: ".StatsDrivingSchool",
    data: {
        active: false,
        maxHealth: "1000",
        minCarHe: "1000",
        bodyHealth: "1000",
        engineHealth: "1000",

    },
    methods: {
        set: function (mh, bh, eh) {
            this.minCarHe = mh;
            this.bodyHealth = bh;
            this.engineHealth = eh;
        }
    }
});

var DrivingSchoolTEST = new Vue({
    el: ".DrivingSchoolTEST",
    data: {
        active: false,
        header: "Тест",
    },
    methods: {

        set: function (m0, m1, m2, m3, m4, m5) {
            this.money0 = m0;
            this.money1 = m1;
            this.money2 = m2;
            this.money3 = m3;
            this.money4 = m4;
            this.money5 = m5;
        },
        exit: function () {
            this.active = false;
            this.str = 0;
            mp.trigger('CloseDrivingSchool');
        },
        select: function (id) {
            mp.trigger("selectSchool", id);
        },
        ok: function (ok) {
            mp.trigger("selectSchoolTEST", ok);
        }

    }
});