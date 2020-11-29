import React from 'react';
import ReactDOM from 'react-dom';

var EventManager = {
  events: {},
  addHandler: function (eventName, handler) {
    if (eventName in this.events) {
      this.events[eventName].push(handler);
    } else {
      this.events[eventName] = [handler];
    }
  },
  removeHandler: function (eventName, handler) {
    if (eventName in this.events) {
      var index = this.events[eventName].indexOf(handler);
      this.events[eventName].splice(index, 1);
    }
  }
};

class App extends React.Component {
  constructor(props) {
    super(props);
  }

  render() {
    return ('iTeffa | Добро пожаловать');
  }
}

ReactDOM.render(<App/>, document.getElementById('app'));