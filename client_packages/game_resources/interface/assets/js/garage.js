function loadItems(items) {
    if (items.length === 0) {
      $('#text-loading').innerHTML = "В вашем гараже нет автомобилей.";
      $('#fahrzeugGUI').hide();
      console.log('abc');
    } else {
      $('#text-loading').hide();
  
      var row, cell, text, r, c,
        prop = ['name', 'fuel', 'color', 'insurance', 'km', ' fuelConsumption', 'buyDate'],
        elementLeft = document.getElementById("menu-loaded");
      elementRight = document.getElementById("fahrzeugGUI");
      for (r = 0; r < items.length; r++) {
        createUI(r, items[r]["name"]);
      }
      $(".menu-body-option").click((event) => {
        var id = $(event.target).data("id");
        var data = items[id];
        generateInformations(data, id);
      });
      generateInformations(items[0], 0);
    }
  }
  
  function createUI(id, name) {
    var element = document.createElement("div");
    element.classList.add("menu-body-option");
    var innerElement = document.createElement('p');
    innerElement.dataset.id = id;
    innerElement.innerHTML = name;
    $("#menu-loaded").append(element);
    $(element).append(innerElement);
  }
  
  function generateInformations(items, id) {
    document.getElementById("vehicleName").innerHTML = items["name"];
    document.getElementById("vehicleFuel").innerHTML = items["fuel"];
    document.getElementById("vehicleColor").innerHTML = items["color"];
    document.getElementById("vehicleInsurance").innerHTML = items["insurance"];
    document.getElementById("vehicleKm").innerHTML = items["km"];
    document.getElementById("vehicleConsumption").innerHTML = items["fuelConsumption"];
    document.getElementById("vehicleBuyDate").innerHTML = items["buyDate"];
    document.getElementById("ausparken").dataset.id = items["id"];
  }
  
  $('#ausparken').click((event) => {
    var id = $(event.target).data("id");
    mp.trigger("spawnVehicle", id);
  });