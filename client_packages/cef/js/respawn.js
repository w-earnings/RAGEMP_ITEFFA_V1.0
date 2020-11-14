function set(data) {
    data = JSON.parse(data);
    $('.box1').css('display', 'flex');

    if (data[1] === true) $('.box2').css('display', 'flex');
    else $('.noBox2').css('display', 'flex');

    if (data[2] === true) $('.box3').css('display', 'flex');
    else $('.noBox3').css('display', 'flex');
}

function spawn(id) {
    mp.trigger('spawn', id);
}