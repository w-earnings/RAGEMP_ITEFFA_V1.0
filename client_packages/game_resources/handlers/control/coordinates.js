// Copyright 2020 iTeffa.com
mp.events.add("render", () => {
  var player = mp.players.local; 
  const pos = player.position;
  const heading = player.getHeading();
  mp.game.graphics.drawText(`x: ${pos.x.toFixed(2)}, y: ${pos.y.toFixed(2)}, z: ${pos.z.toFixed(2)}, h: ${heading.toFixed(2)}`, [0.55, 0.965], {
	font: 0,
	color: [255, 255, 255, 230],
	scale: [0.4, 0.4],
	outline: true
  });   
});
