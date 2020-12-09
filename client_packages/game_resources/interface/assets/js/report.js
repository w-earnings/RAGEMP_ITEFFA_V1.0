var app = new Vue({
    el: '#app',
    data: {
      active: false,
      playerName: "",
      selected: false,
      answer: "",
      reports: []
    },
    methods: {
      addReport(report) {
        this.reports.unshift(report);
      },
      selectReport(report) {
        if (report.blocked) return;
        if (this.selected) return;
        this.selected = report;
        this.selected.blocked = true;
        this.selected.blockedBy = this.playerName;
        this.$refs.answerBox.focus();
        mp.trigger("takereport", report.id, false);
      },
      setStatus(id, name) {
        let report = this.reports.find(r => r.id == id);
        if (!report) return;
        report.blocked = true;
        report.blockedBy = name;
      },
      returnReport(report) {
        if (!report) return;
        this.selected.blocked = false;
        this.selected.blockedBy = "";
        this.selected = false;
        this.answer = "";
        mp.trigger("takereport", report.id, true);
      },
      sendAnswer(report) {
        if (!this.answer) return;
        this.selected = false;
        mp.trigger("sendreport", report.id, this.answer);
        this.answer = "";
      },
      unblockReport(id) {
        let report = this.reports.find(r => r.id == id);
        if (!report) return;
        report.blocked = false;
        report.blockedBy = "";
      },
      deleteReport(id) {
        let reportToDelete = this.reports.findIndex(r => r.id == id);
        if (reportToDelete < 0) return;
        this.reports.splice(reportToDelete, 1);
      },
      exitReport() {
        this.active = false;
        mp.trigger('exitreport');
      }
    }
  })
  
  function addReport(id_, author_, text_, blocked_, blockedBy_) {
    let report = {
      id: id_,
      author: author_,
      text: text_,
      blocked: blocked_,
      blockedBy: blockedBy_
    }
    app.addReport(report);
  }
  
  function deleteReport(id) {
    app.deleteReport(id);
  }
  
  function setStatus(id, name) {
    if (name.length < 1) {
      app.unblockReport(id);
    } else {
      app.setStatus(id, name);
    }
  }