U.idToLetter = (id) => {
	if(id == 3) return 'R';
	if(id == 2) return 'M';

	return 'L';
};

U.slotNameById = (id) => {
	return 'slot' + U.idToLetter(id);
};

var slots = new Vue({
	el: ".authDiv",
	data: function () {
		return{
			server: 0,
			slotL: ["", "", 7, 0, 0, "", 0, 0],
			slotM: ["", "", 7, 0, 0, "", 0, 0],
			slotR: ["", "", 7, 0, 0, "", 0, 0],
			blockL: ["Бан", "Cam", "then", "when"],
			blockM: ["Бан", "Cam", "then", "when"],
			blockR: ["Бан", "Cam", "then", "when"],
			uuids: [0, 0, 0],
			redbucks: -1,
			login: "username",
			currSlotId: 0,
			currSlot: ["", "", 7, 0, 0, "", 0, 0],
			emptySlot: ["", "", 7, 0, 0, "", 0, 0],

			is_modal: false,

			show_modal_new_char: false,
			disable_new_char_btn: false,

			show_modal_delete: false,

			chooseBuy: false,
			canChoose: false,
		};
	},
	methods: {
		deleteChar: () => {
			U.trigger('delChar', slots.currSlotId, '', '', '');
		}
	}
})

function checkCode(str) {
	let ascii;
	for (let i = 0; i != str.length; i++) {
		ascii = str.charCodeAt(i);
		if (ascii < 48 || ascii > 57) return false;
	}
	return true;
}

function toslots(data) {
	regPage.removeClass('show');
	authPage.removeClass('show');
	restPage.removeClass('show');
	slotsPage.addClass('show');

	console.log(data);
	data = JSON.parse(data);
	if (data[0] == -1) {
		slotL.addClass('free');
	} else {
		if (data[0][0] == 'ban') {
			slots.blockL[0] = data[0][1];
			slots.blockL[1] = data[0][2];
			slots.blockL[2] = data[0][3];
			slots.blockL[3] = data[0][4];

			slotL.addClass('free blocked');
		} else {

			slots.slotL[0] = data[0][0];
			slots.slotL[1] = data[0][1];
			slots.slotL[2] = data[0][2];
			slots.slotL[3] = data[0][3];
			slots.slotL[4] = 3 + data[0][2] * 3;
			slots.slotL[5] = data[0][4];
			slots.slotL[6] = data[0][5];
			slots.slotL[7] = data[0][6];
			slots.uuids[0] = data[0][7];

			slotL.addClass('active');

			slots.currSlotId = 1;
			slots.currSlot = slots.slotL;
			slots.canChoose = true;
		}
	}

	if (data[1] == -1) {
		slotM.addClass('free');
	} else {
		if (data[1][0] == 'ban') {
			slots.blockM[0] = data[1][1];
			slots.blockM[1] = data[1][2];
			slots.blockM[2] = data[1][3];
			slots.blockM[3] = data[1][4];

			slotM.addClass('free blocked');
		} else {
			slots.slotM[0] = data[1][0];
			slots.slotM[1] = data[1][1];
			slots.slotM[2] = data[1][2];
			slots.slotM[3] = data[1][3];
			slots.slotM[4] = 3 + data[1][2] * 3;
			slots.slotM[5] = data[1][4];
			slots.slotM[6] = data[1][5];
			slots.slotM[7] = data[1][6];
			slots.uuids[1] = data[1][7];

			slotM.addClass('active');
		}
	}

	if (data[2] === -1) {
		slotR.addClass('free');
	} else if (data[2] === -2) {
		slotR.addClass('non-active');
	} else {
		if (data[2][0] == 'ban') {
			slots.blockR[0] = data[2][1];
			slots.blockR[1] = data[2][2];
			slots.blockR[2] = data[2][3];
			slots.blockR[3] = data[2][4];

			slotR.addClass('free blocked');
		} else {
			slots.slotR[0] = data[2][0];
			slots.slotR[1] = data[2][1];
			slots.slotR[2] = data[2][2];
			slots.slotR[3] = data[2][3];
			slots.slotR[4] = 3 + data[2][2] * 3;
			slots.slotR[5] = data[2][4];
			slots.slotR[6] = data[2][5];
			slots.slotR[7] = data[2][6];
			slots.uuids[2] = data[2][7];

			slotR.addClass('active');
		}
	}

	slots.redbucks = data[3];
	slots.login = data[4];

	slotL.addClass('active');
	slotM.addClass('active');
	slotR.addClass('active');

	U.trigger('rederchar');

	//U.trigger('chooseChar', slots.uuids[0]);
}

function unlockSlot(data) {
	slotR.removeClass('non-active');
	slotR.addClass('free');
	slots.redbucks = data;
}

function delchar(data) {
	slots.show_modal_delete = slots.is_modal = false;
	slots.currSlot = slots.emptySlot;
	slots.currSlotId = 0;
	slots.canChoose = false;

	switch (data) {
		case 1:
			slots.slotL = slots.emptySlot;
			slotL.addClass('free');
			return;
		case 2:
			slots.slotM = slots.emptySlot;
			slotM.addClass('free');
			return;
		case 3:
			slots.slotR = slots.emptySlot;
			slotR.addClass('free');
			return;
	}
}

var restPassState = false;
var restPass = null;
var registerBtn = null;
var restoreBtn = null;
var authBackBtn = null;
var authBtn = null;
var endRegisterBtn = null;
var endRestoreBtn = null;
var authPage = null;
var regPage = null;
var restPage = null;
var slotsPage = null;
var slotL = null;
var slotM = null;
var slotR = null;

$(document).ready(() => {
	restPass = $('.entry-login');
	restoreBtn = $('.js-btn-restore');
	registerBtn = $('.js-btn-register');
	authBackBtn = $('.js-btn-back');
	authBtn = $('.js-btn-auth');
	endRegisterBtn = $('.btn-register-end');
	endRestoreBtn = $('.btn-restore-end');
	authPage = $('.auth-page');
	regPage = $('.reg-page');
	restPage = $('.rest-page');
	slotsPage = $('.slots-page');
	slotL = $('.col-l');
	slotM = $('.col-m');
	slotR = $('.col-r');

	// Переход на страницу авторизации (клик на "Регистрация")
	registerBtn.on('click', (e) => {
		e.preventDefault();
		authPage.removeClass('show');
		restPage.removeClass('show');
		regPage.addClass('show');
	});

	// Переход на страницу восстановления пароля (клик на 'Восстановить')
	restoreBtn.on('click', (e) => {
		e.preventDefault();
		authPage.removeClass('show');
		regPage.removeClass('show');
		restPage.addClass('show');
	});

	// Возврат на страницу авторизации (клик на "Назад")
	authBackBtn.on('click', (e) => {
		e.preventDefault();
		if (restPassState) {
			restPassState = false;
			restPass.attr('placeholder', 'Логин / E-mail');
		}
		regPage.removeClass('show');
		restPage.removeClass('show');
		authPage.addClass('show');
	});

	// Сохраниние данных с полей (Авторизация - Кнопка "Войти")
	authBtn.on('click', () => {
		let authData = $('#auth-form').toJSON();
		mp.trigger('signin', authData);
		localStorage['form_data'] = authData;
		return false;
	});

	// Сохраниние данных с полей (Регистрация - Кнопка "Зарегистрироваться")
	endRegisterBtn.on('click', () => {
		let regData = $('#reg-form').toJSON();
		mp.trigger('signup', regData);
		localStorage['form_data'] = regData;
		return false;
	});


	// Сохранение данных с полей (Восстановление пароля - Кнопка "Вспомнить")
	endRestoreBtn.on('click', (e) => {
		let regData = $('#rest-form').toJSON();
		let myval = document.getElementById("rest-form").elements[0].value;
		if (!restPassState) {
			e.preventDefault();
			if (myval.length != 0) {
				restPassState = true;
				restPass.attr('placeholder', 'Код из письма');
				restPass.val('');
				mp.trigger('restorepass', 0, regData);
				localStorage['form_data'] = regData;
			}
		} else {
			if (myval.length == 4) {
				if (checkCode(myval)) mp.trigger('restorepass', 1, regData);
				else restPass.val('');
			} else restPass.val('');
		}
		return false;
	});

	 // Кнопка "Войти"
    $('.js-btn-enter-1').on('click', () => {
        mp.trigger('selectChar', 1);
        return false;
    });

    $('.js-btn-enter-2').on('click', () => {
        mp.trigger('selectChar', 2);
        return false;
    });

    $('.js-btn-enter-3').on('click', () => {
        mp.trigger('selectChar', 3);
        return false;
    });

    // Кнопка "Разблокировать слот"
    $('.js-unlock-slot-1').on('click', () => {
        mp.trigger('buyNewSlot', 1);
        return false;
    });

    $('.js-unlock-slot-2').on('click', () => {
        mp.trigger('buyNewSlot', 2);
        return false;
    });

    $('.js-unlock-slot-3').on('click', () => {
        mp.trigger('buyNewSlot', 3);
        return false;
    });

    // Кнопка "Перенос персонажа"
    $('.js-transfer-person-submit-1').on('click', () => {
        let data = $('#transfer-person-1').toJSON();
        data = JSON.parse(data);
        mp.trigger('transferChar', 1, data["transfer-person-1__name"], data["transfer-person-1__sername"], data["transfer-person-1__pw"]);
        return false;
    });

    $('.js-transfer-person-submit-2').on('click', () => {
        let data = $('#transfer-person-2').toJSON();
        data = JSON.parse(data);
        console.log(`transfer 2 activated ${data["transfer-person-2__name"]} ${data["transfer-person-2__sername"]} ${data["transfer-person-2__pw"]}`)
        mp.trigger('transferChar', 2, data["transfer-person-2__name"], data["transfer-person-2__sername"], data["transfer-person-2__pw"]);
        return false;
    });

    $('.js-transfer-person-submit-3').on('click', () => {
        let data = $('#transfer-person-3').toJSON();
        data = JSON.parse(data);
        mp.trigger('transferChar', 3, data["transfer-person-3__name"], data["transfer-person-3__sername"], data["transfer-person-3__pw"]);
        return false;
    });

    // Кнопка "Создание персонажа"
    $('.js-create-person-submit-1').on('click', () => {
        let data = $('#create-person-1').toJSON();
        data = JSON.parse(data);
        mp.trigger('newChar', 1, data["new-person-1__name"], data["new-person-1__sername"]);
        return false;
    });

    $('.js-create-person-submit-2').on('click', () => {
        let data = $('#create-person-2').toJSON();
        data = JSON.parse(data);
        mp.trigger('newChar', 2, data["new-person-2__name"], data["new-person-2__sername"]);
        return false;
    });

    $('.js-create-person-submit-3').on('click', () => {
        let data = $('#create-person-3').toJSON();
        data = JSON.parse(data);
        mp.trigger('newChar', 3, data["new-person-3__name"], data["new-person-3__sername"]);
        return false;
    });

    // Кнопка "Удалить персонажа" (отдельно на каждый слот)
    $('.js-btn-delete-1').on('click', (e) => {
        e.preventDefault();
        $('.col-l').addClass('delete opacity');
    });

    $('.js-btn-delete-2').on('click', (e) => {
        e.preventDefault();
        $('.col-m').addClass('delete opacity');
    });

    $('.js-btn-delete-3').on('click', (e) => {
        e.preventDefault();
        $('.col-r').addClass('delete opacity');
    });

    // Кнопка "Отмена" при удаления персонажа (отдельно на каждый слот)
    $('.js-delete-cancel-1').on('click', (e) => {
        e.preventDefault();
        $('.col-l').removeClass('delete opacity');
    });

    $('.js-delete-cancel-2').on('click', (e) => {
        e.preventDefault();
        $('.col-m').removeClass('delete opacity');
    });

    $('.js-delete-cancel-3').on('click', (e) => {
        e.preventDefault();
        $('.col-r').removeClass('delete opacity');
    });
// col-l
    $('.js-create-new-1').on('click', (e) => {
        e.preventDefault();
        $('.col-l').addClass('create');
    });

    $('.js-create-cancel-1').on('click', (e) => {
        e.preventDefault();
        $('.col-l').removeClass('create');
    });

    $('.js-transfer-person-1').on('click', (e) => {
        e.preventDefault();
        $('.col-l').addClass('transfer');
    });

    $('.js-transfer-cancel-1').on('click', (e) => {
        e.preventDefault();
        $('.col-l').removeClass('transfer');
    });

    // col-m
    $('.js-create-new-2').on('click', (e) => {
        e.preventDefault();
        $('.col-m').addClass('create');
    });

    $('.js-create-cancel-2').on('click', (e) => {
        e.preventDefault();
        $('.col-m').removeClass('create');
    });

    $('.js-transfer-person-2').on('click', (e) => {
        e.preventDefault();
        $('.col-m').addClass('transfer');
    });

    $('.js-transfer-cancel-2').on('click', (e) => {
        e.preventDefault();
        $('.col-m').removeClass('transfer');
    });

    // col-r
    $('.js-create-new-3').on('click', (e) => {
        e.preventDefault();
        $('.col-r').addClass('create');
    });

    $('.js-create-cancel-3').on('click', (e) => {
        e.preventDefault();
        $('.col-r').removeClass('create');
    });

    $('.js-transfer-person-3').on('click', (e) => {
        e.preventDefault();
        $('.col-r').addClass('transfer');
    });

    $('.js-transfer-cancel-3').on('click', (e) => {
        e.preventDefault();
        $('.col-r').removeClass('transfer');
    });
});