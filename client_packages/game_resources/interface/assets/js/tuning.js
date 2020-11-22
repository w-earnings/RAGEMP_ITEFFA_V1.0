function disable(json) {
    json.forEach((item, i, arr) => {
        $('#' + item).hide();
    });
}

function set(speed, brakes, boost, clutch) {
    $('#speed_bar').attr('value', speed);
    $('#brakes_bar').attr('value', brakes);
    $('#boost_bar').attr('value', boost);
    $('#clutch_bar').attr('value', clutch);
}

$('a').click((e) => {
    mp.trigger('tpage', e.currentTarget.id);
});

$('div.item').hover((e) => hover(e.currentTarget.id), () => {});
$('div.item').click((e) => click(e.currentTarget.id));

function click(id) {
    if (!id) return;
    mp.trigger('tclk', id);
}

function hover(id) {
    mp.trigger('thov', id);
}

function price(obj) {
    if (Array.isArray(obj[0])) {
        obj.forEach((item, i, arr) => {
            $(`#${item[0]} p`)[1].innerHTML = item[1];
        });
    } else {
        $(`#${obj[0]} p`)[1].innerHTML = obj[1];
    }
}

function add(obj) {
    if (Array.isArray(obj[0])) {
        obj.forEach((item, i, arr) => {
            $('.container').append(`<div id='${item[0]}' class='item'><div className="price"><p>${item[2]}</p></div><div className="buy-button"><div className="img"></div><p>${item[1]}</p></div>`);
            $('#' + item[0]).hover((e) => hover(e.currentTarget.id), () => {});
            $('#' + item[0]).click((e) => click(e.currentTarget.id));
        });
    } else {
        $('.content_box').append(`<div id='${obj[0]}' class='item'><div className="price"><p>${obj[2]}</p></div><div className="buy-button"><div className="img"></div><p>${obj[1]}</p></div>`)
        $('#' + obj[0]).hover((e) => hover(e.currentTarget.id), () => {});
        $('#' + obj[0]).click((e) => click(e.currentTarget.id));
    }
}

function show(state) {
    if (!state) {
        $('body').hide();
    } else {
        $('body').show();
    }
}
