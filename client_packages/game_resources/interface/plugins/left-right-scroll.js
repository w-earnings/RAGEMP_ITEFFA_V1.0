$('#right-button').click(function() {
  event.preventDefault();
  $('#content').animate({
    scrollLeft: "+=160px"
  }, "slow");
});

 $('#left-button').click(function() {
  event.preventDefault();
  $('#content').animate({
    scrollLeft: "-=160px"
  }, "slow");
});