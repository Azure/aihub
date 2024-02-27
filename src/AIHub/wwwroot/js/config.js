!function() {
    var t = sessionStorage.getItem("__AOAIDemo__")
      , e = document.getElementsByTagName("html")[0]
      , i = {
        theme: "light",
        nav: "vertical",
        layout: {
            mode: "fluid",
            position: "fixed"
        },
        topbar: {
            color: "light"
        },
        menu: {
            color: "dark"
        },
        sidenav: {
            size: "default",
            user: !1
        }
    }
      , o = (this.html = document.getElementsByTagName("html")[0],
    config = Object.assign(JSON.parse(JSON.stringify(i)), {}),
    this.html.getAttribute("data-bs-theme"))
      , o = (config.theme = null !== o ? o : i.theme,
    this.html.getAttribute("data-layout"))
      , o = (config.nav = null !== o ? "topnav" === o ? "horizontal" : "vertical" : i.nav,
    this.html.getAttribute("data-layout-mode"))
      , o = (config.layout.mode = null !== o ? o : i.layout.mode,
    this.html.getAttribute("data-layout-position"))
      , o = (config.layout.position = null !== o ? o : i.layout.position,
    this.html.getAttribute("data-topbar-color"))
      , o = (config.topbar.color = null != o ? o : i.topbar.color,
    this.html.getAttribute("data-sidenav-size"))
      , o = (config.sidenav.size = null !== o ? o : i.sidenav.size,
    this.html.getAttribute("data-sidenav-user"))
      , o = (config.sidenav.user = null !== o || i.sidenav.user,
    this.html.getAttribute("data-menu-color"));
    if (config.menu.color = null !== o ? o : i.menu.color,
    window.defaultConfig = JSON.parse(JSON.stringify(config)),
    null !== t && (config = JSON.parse(t)),
    window.config = config,
    "topnav" === e.getAttribute("data-layout") ? config.nav = "horizontal" : config.nav = "vertical",
    config && (e.setAttribute("data-bs-theme", config.theme),
    e.setAttribute("data-layout-mode", config.layout.mode),
    e.setAttribute("data-menu-color", config.menu.color),
    e.setAttribute("data-topbar-color", config.topbar.color),
    e.setAttribute("data-layout-position", config.layout.position),
    "vertical" == config.nav)) {
        let t = config.sidenav.size;
        window.innerWidth <= 767 ? t = "full" : 767 <= window.innerWidth && window.innerWidth <= 1140 && "full" !== self.config.sidenav.size && "fullscreen" !== self.config.sidenav.size && (t = "condensed"),
        e.setAttribute("data-sidenav-size", t),
        config.sidenav.user && "true" === config.sidenav.user.toString() ? e.setAttribute("data-sidenav-user", !0) : e.removeAttribute("data-sidenav-user")
    }
}();
