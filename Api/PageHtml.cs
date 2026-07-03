namespace Jellyfin.Plugin.YouTubeFast.Api;

/// <summary>Static HTML for the self-service management page (Space Grotesk theme).</summary>
internal static class PageHtml
{
    public const string Html = """
<!DOCTYPE html>
<html lang="fr">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>YouTubeFast</title>
<link rel="icon" type="image/svg+xml" href="data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'><stop offset='0' stop-color='%2322c3b6'/><stop offset='1' stop-color='%232a9fd6'/></linearGradient></defs><rect width='24' height='24' rx='6' fill='url(%23g)'/><g fill='%23ffffff'><polygon points='4,6 11,12 4,18'/><polygon points='12,6 19,12 12,18'/></g></svg>" />
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;600;700&display=swap" rel="stylesheet">
<style>
  :root {
    --bg:#0c1116; --bg-elev:#141b22; --bg-elev2:#1b242d; --border:#243039;
    --text:#e9eff2; --text-dim:#8aa0ab; --accent:#22c3b6; --accent-2:#2a9fd6;
    --on-accent:#042522; --glow:rgba(34,195,182,0.25); --card-shadow:0 8px 30px rgba(0,0,0,0.35);
  }
  :root.light {
    --bg:#eef3f4; --bg-elev:#ffffff; --bg-elev2:#f2f7f8; --border:#dbe5e8;
    --text:#13232a; --text-dim:#5b7079; --accent:#0d9488; --accent-2:#1593b8;
    --on-accent:#ffffff; --glow:rgba(13,148,136,0.16); --card-shadow:0 8px 30px rgba(20,50,60,0.10);
  }
  * { box-sizing: border-box; }
  html, body { margin:0; padding:0; }
  body {
    font-family:'Space Grotesk', system-ui, sans-serif; background:var(--bg); color:var(--text);
    min-height:100vh; transition:background .3s, color .3s;
  }
  .glow { position:fixed; inset:0; pointer-events:none; overflow:hidden; z-index:0; }
  .glow div { position:absolute; top:-200px; left:50%; transform:translateX(-50%);
    width:760px; height:520px; background:radial-gradient(ellipse at center, var(--glow), transparent 70%); filter:blur(20px); }
  .wrap { position:relative; z-index:1; }
  .hidden { display:none !important; }

  input { font-family:inherit; }
  input::placeholder { color:var(--text-dim); opacity:.8; }
  input:focus { outline:none; border-color:var(--accent) !important; box-shadow:0 0 0 3px var(--glow); }
  .field {
    width:100%; padding:13px 15px; border-radius:11px; border:1px solid var(--border);
    background:var(--bg-elev2); color:var(--text); font-size:14.5px;
  }
  .btn-accent {
    border:none; border-radius:11px; background:linear-gradient(135deg,var(--accent),var(--accent-2));
    color:var(--on-accent); font-family:inherit; font-weight:700; cursor:pointer;
    box-shadow:0 4px 14px var(--glow); transition:transform .12s, box-shadow .2s;
  }
  .btn-accent:hover { transform:translateY(-1px); box-shadow:0 10px 26px var(--glow); }
  .btn-ghost {
    border:1px solid var(--border); border-radius:11px; background:var(--bg-elev);
    color:var(--text); font-family:inherit; font-weight:600; cursor:pointer; transition:border-color .15s, color .15s;
  }
  .btn-ghost:hover { border-color:var(--accent); color:var(--accent); }

  .logo { border-radius:13px; background:linear-gradient(135deg,var(--accent),var(--accent-2));
    display:flex; align-items:center; justify-content:center; box-shadow:0 6px 20px var(--glow); flex:none; }
  .brandName { font-weight:700; letter-spacing:1.5px; color:var(--text); }
  .brandName b { color:var(--accent); font-weight:700; }

  .panel { background:var(--bg-elev); border:1px solid var(--border); border-radius:18px;
    padding:22px; box-shadow:var(--card-shadow); }
  .label { display:block; font-size:12px; font-weight:600; letter-spacing:.4px;
    color:var(--text-dim); margin-bottom:7px; text-transform:uppercase; }

  .avatar { width:38px; height:38px; border-radius:50%; flex:none; display:flex;
    align-items:center; justify-content:center; font-weight:700; font-size:15px; }
  .rowline { display:flex; align-items:center; gap:13px; }
  .name { flex:1; min-width:0; font-weight:600; color:var(--text);
    white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }

  .pill { font-size:12.5px; font-weight:600; padding:4px 11px; border-radius:999px;
    background:var(--bg-elev2); border:1px solid var(--border); color:var(--text-dim); }
  .tab { flex:1; padding:11px 14px; border-radius:12px; border:1px solid var(--border);
    background:var(--bg-elev); color:var(--text-dim); font-family:inherit; font-weight:600;
    font-size:14px; cursor:pointer; transition:border-color .15s, color .15s, background .15s; }
  .tab:hover { border-color:var(--accent); color:var(--accent); }
  .tab.active { background:var(--glow); border-color:var(--accent); color:var(--accent); }
  .shorts { display:flex; align-items:center; gap:7px; padding:7px 13px; border-radius:999px;
    font-family:inherit; font-size:12.5px; font-weight:600; cursor:pointer; border:1px solid var(--border);
    background:transparent; color:var(--text-dim); flex:none; }
  .shorts.on { border-color:var(--accent); background:var(--glow); color:var(--accent); }
  .dot { width:8px; height:8px; border-radius:50%; background:var(--text-dim); }
  .shorts.on .dot { background:var(--accent); box-shadow:0 0 8px var(--accent); }
  .remove { padding:8px 14px; border-radius:10px; border:1px solid var(--border); background:transparent;
    color:var(--text-dim); font-family:inherit; font-size:13px; font-weight:600; cursor:pointer; flex:none; }
  .remove:hover { border-color:#e0738a; color:#e0738a; }

  .toggle { position:fixed; top:20px; right:22px; z-index:20; display:flex; align-items:center; gap:9px;
    padding:8px 14px; border-radius:999px; border:1px solid var(--border); background:var(--bg-elev);
    color:var(--text); font-family:inherit; font-size:13px; font-weight:500; cursor:pointer; box-shadow:var(--card-shadow); }
  .toggle:hover { border-color:var(--accent); }

  .overlay { position:fixed; inset:0; background:rgba(0,0,0,.6); display:flex; align-items:center;
    justify-content:center; z-index:30; backdrop-filter:blur(2px); }
  .modal { background:var(--bg-elev); border:1px solid var(--border); border-radius:18px; padding:26px;
    max-width:380px; text-align:center; box-shadow:0 10px 34px rgba(0,0,0,.5); margin:0 1rem; }
  .toast { position:fixed; bottom:22px; left:50%; transform:translateX(-50%); z-index:40;
    background:linear-gradient(135deg,var(--accent),var(--accent-2)); color:var(--on-accent);
    padding:12px 20px; border-radius:12px; font-weight:600; font-size:13.5px; box-shadow:0 8px 22px var(--glow);
    animation:fade .3s ease; }
  @keyframes fade { from { opacity:0; transform:translate(-50%,8px); } to { opacity:1; transform:translate(-50%,0); } }
  @keyframes spin { to { transform:rotate(360deg); } }
  .spin { animation:spin .9s linear infinite; }

  @media (max-width:560px) {
    .rowline { flex-wrap:wrap; }
    .name { flex-basis:100%; }
    .searchRow { flex-wrap:wrap; }
    .searchRow .field { flex-basis:100%; }
  }
</style>
</head>
<body>

<div class="glow"><div></div></div>

<button class="toggle hidden" id="themeBtn">
  <svg id="themeIcon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="var(--accent)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></svg>
  <span id="themeLabel"></span>
</button>

<div class="wrap">

  <!-- LOGIN -->
  <div id="login" style="min-height:100vh; display:flex; flex-direction:column; align-items:center; justify-content:center; padding:40px 20px;">
    <div style="display:flex; align-items:center; gap:13px; margin-bottom:34px;">
      <div class="logo" style="width:46px; height:46px;">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="#fff"><polygon points="2,4.5 11,12 2,19.5"/><polygon points="12,4.5 21,12 12,19.5"/></svg>
      </div>
      <div style="display:flex; flex-direction:column; line-height:1.05;">
        <span class="brandName" style="font-size:19px;">YOUTUBE <b>FAST</b></span>
        <span style="font-size:10.5px; font-weight:500; letter-spacing:2.5px; color:var(--text-dim);">PLUGIN JELLYFIN</span>
      </div>
    </div>

    <div class="panel" style="width:100%; max-width:392px; border-radius:20px; padding:32px 30px;">
      <h1 style="margin:0 0 5px; font-size:25px; font-weight:700; letter-spacing:-.3px;">Connexion</h1>
      <p style="margin:0 0 24px; font-size:14px; color:var(--text-dim);">Utilisez votre compte Jellyfin.</p>

      <label class="label">Nom d'utilisateur</label>
      <input id="u" class="field" style="margin-bottom:17px" placeholder="Votre identifiant" />

      <label class="label">Mot de passe</label>
      <input id="p" type="password" class="field" style="margin-bottom:24px" placeholder="••••••••" />

      <button id="loginBtn" class="btn-accent" style="width:100%; padding:14px; font-size:15px;">Se connecter</button>
      <p id="loginErr" style="margin:14px 0 0; font-size:13px; color:#e0738a;"></p>
    </div>
    <p style="margin:26px 0 0; font-size:12.5px; color:var(--text-dim);">Synchronisez vos chaînes YouTube dans Jellyfin, sans les Shorts.</p>
  </div>

  <!-- APP -->
  <div id="app" class="hidden" style="max-width:760px; margin:0 auto; padding:30px 22px 70px;">
    <div style="display:flex; align-items:center; gap:11px; margin-bottom:26px;">
      <div class="logo" style="width:34px; height:34px; border-radius:10px;">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="#fff"><polygon points="2,4.5 11,12 2,19.5"/><polygon points="12,4.5 21,12 12,19.5"/></svg>
      </div>
      <span class="brandName" style="font-size:14px;">YOUTUBE <b>FAST</b></span>
    </div>

    <div style="display:flex; align-items:flex-end; justify-content:space-between; gap:16px; margin-bottom:24px;">
      <div>
        <h1 style="margin:0 0 4px; font-size:31px; font-weight:700; letter-spacing:-.6px;">Ma bibliothèque YouTube</h1>
        <p style="margin:0; font-size:14px; color:var(--text-dim);">Gérez vos chaînes et vos vidéos synchronisées dans Jellyfin.</p>
      </div>
      <button id="logoutBtn" class="btn-ghost" style="flex-shrink:0; padding:10px 16px; font-size:13.5px;">Déconnexion</button>
    </div>

    <div class="tabs" style="display:flex; gap:8px; margin-bottom:22px;">
      <button id="tabBtnChaine" class="tab active" type="button">Chaîne</button>
      <button id="tabBtnVideo" class="tab" type="button">Vidéo</button>
    </div>

    <!-- TAB: CHAÎNE (fonctionnalité existante, inchangée) -->
    <div id="tabChaine">
    <div class="panel" style="margin-bottom:26px;">
      <div style="display:flex; align-items:center; gap:8px; margin-bottom:15px;">
        <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="var(--accent)" stroke-width="2.2" stroke-linecap="round"><circle cx="11" cy="11" r="7"/><line x1="21" y1="21" x2="16.5" y2="16.5"/></svg>
        <h2 style="margin:0; font-size:16px; font-weight:600;">Ajouter une chaîne</h2>
      </div>
      <div class="searchRow" style="display:flex; gap:10px;">
        <input id="q" class="field" style="flex:1;" placeholder="Rechercher une chaîne YouTube…" />
        <button id="searchBtn" class="btn-accent" style="flex-shrink:0; padding:13px 22px; font-size:14px;">Rechercher</button>
      </div>
      <div id="results" style="display:flex; flex-direction:column; gap:8px; margin-top:18px; max-height:430px; overflow-y:auto;"></div>
    </div>

    <div style="display:flex; align-items:center; justify-content:space-between; margin-bottom:13px;">
      <h2 style="margin:0; font-size:17px; font-weight:600;">Mes chaînes</h2>
      <span id="count" class="pill">0 chaîne</span>
    </div>
    <div id="mine" style="display:flex; flex-direction:column; gap:10px; margin-bottom:28px;"></div>
    </div><!-- /tabChaine -->

    <!-- TAB: VIDÉO -->
    <div id="tabVideo" class="hidden">
      <div class="panel" style="margin-bottom:26px;">
        <div style="display:flex; align-items:center; gap:8px; margin-bottom:15px;">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="var(--accent)" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="4" width="20" height="16" rx="3"/><polygon points="10,9 16,12 10,15" fill="var(--accent)" stroke="none"/></svg>
          <h2 style="margin:0; font-size:16px; font-weight:600;">Ajouter une vidéo</h2>
        </div>
        <div class="searchRow" style="display:flex; gap:10px;">
          <input id="vq" class="field" style="flex:1;" placeholder="Collez un lien de vidéo YouTube…" />
          <button id="addVideoBtn" class="btn-accent" style="flex-shrink:0; padding:13px 22px; font-size:14px;">Ajouter</button>
        </div>
        <p id="vmsg" style="margin:11px 0 0; font-size:13px; color:var(--text-dim); min-height:1px;"></p>
      </div>

      <div style="display:flex; align-items:center; justify-content:space-between; margin-bottom:13px;">
        <h2 style="margin:0; font-size:17px; font-weight:600;">Vidéo dans ma bibliothèque</h2>
        <span id="vcount" class="pill">0 vidéo</span>
      </div>
      <div id="videos" style="display:flex; flex-direction:column; gap:10px; margin-bottom:28px;"></div>
    </div>

    <div style="display:flex; align-items:center; gap:16px;">
      <button id="syncBtn" class="btn-accent" style="display:flex; align-items:center; gap:10px; padding:14px 24px; font-size:15px; border-radius:13px;">
        <svg id="syncIcon" width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12a9 9 0 1 1-3-6.7"/><polyline points="21 3 21 9 15 9"/></svg>
        Synchroniser maintenant
      </button>
      <span id="syncStatus" style="font-size:13px; color:var(--text-dim);">Jamais synchronisé</span>
    </div>
  </div>
</div>

<div id="confirmModal" class="overlay hidden">
  <div class="modal">
    <p style="margin:0 0 4px; font-size:16px; font-weight:600;">Avez-vous ajouté tous les youtubeurs de votre choix ?</p>
    <div style="display:flex; gap:12px; justify-content:center; margin-top:18px;">
      <button id="confirmYes" class="btn-accent" style="padding:11px 26px;">Oui</button>
      <button id="confirmNo" class="btn-ghost" style="padding:11px 26px;">Non</button>
    </div>
  </div>
</div>

<script>
(function () {
  var token = localStorage.getItem("ytf_token");
  var userId = localStorage.getItem("ytf_uid");
  var userName = localStorage.getItem("ytf_uname");
  var PALETTE = ['#22c3b6','#2a9fd6','#7c6cf0','#e0738a','#e0a13b','#5bbf6a','#d76b4b','#4aa3a0','#b06fd6'];

  var SUN = '<circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M2 12h2M20 12h2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M19.1 4.9l-1.4 1.4M6.3 17.7l-1.4 1.4"/>';
  var MOON = '<path d="M21 12.8A9 9 0 1 1 11.2 3 7 7 0 0 0 21 12.8z"/>';

  function show(id, on) { document.getElementById(id).classList.toggle("hidden", !on); }
  function esc(s){ return (s||"").replace(/[&<>"]/g,function(m){return ({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;"})[m];}); }

  function applyTheme(t) {
    document.documentElement.classList.toggle("light", t === "light");
    document.getElementById("themeIcon").innerHTML = t === "light" ? MOON : SUN;
    document.getElementById("themeLabel").textContent = t === "light" ? "Mode sombre" : "Mode clair";
    localStorage.setItem("ytf_theme", t);
  }

  function avatar(name, i, img) {
    var c = PALETTE[i % PALETTE.length];
    var d = document.createElement("div");
    d.className = "avatar";
    function fallback() {
      d.textContent = "";
      d.style.overflow = "visible";
      d.style.background = c + "26"; d.style.color = c; d.style.border = "1px solid " + c + "40";
      d.textContent = (name || "?").trim().charAt(0).toUpperCase();
    }
    if (img) {
      d.style.overflow = "hidden";
      d.style.background = "var(--bg-elev2)";
      d.style.border = "1px solid var(--border)";
      var im = document.createElement("img");
      im.src = img; im.alt = name || ""; im.loading = "lazy"; im.referrerPolicy = "no-referrer";
      im.style.cssText = "width:100%;height:100%;object-fit:cover;display:block;";
      im.addEventListener("error", function () { if (im.parentNode === d) d.removeChild(im); fallback(); });
      d.appendChild(im);
    } else {
      fallback();
    }
    return d;
  }

  function api(path, opts) {
    opts = opts || {};
    opts.headers = Object.assign({ "X-Emby-Token": token, "Content-Type": "application/json" }, opts.headers || {});
    return fetch(path, opts).then(function (r) {
      if (!r.ok) { throw new Error(r.status); }
      return r.status === 204 ? null : r.json();
    });
  }

  function login() {
    var u = document.getElementById("u").value, p = document.getElementById("p").value;
    fetch("/Users/AuthenticateByName", {
      method: "POST",
      headers: { "Content-Type": "application/json",
        "Authorization": 'MediaBrowser Client="YouTube Fast", Device="Web", DeviceId="ytf-manager", Version="1.0.0"' },
      body: JSON.stringify({ Username: u, Pw: p })
    }).then(function (r){ if(!r.ok) throw new Error("auth"); return r.json(); })
      .then(function (d) {
        token = d.AccessToken; userId = d.User.Id; userName = d.User.Name;
        localStorage.setItem("ytf_token", token);
        localStorage.setItem("ytf_uid", userId);
        localStorage.setItem("ytf_uname", userName);
        enterApp();
      }).catch(function () {
        document.getElementById("loginErr").textContent = "Connexion échouée. Vérifiez vos identifiants.";
      });
  }

  function logout() {
    localStorage.removeItem("ytf_token"); localStorage.removeItem("ytf_uid"); localStorage.removeItem("ytf_uname");
    token = userId = userName = null;
    show("app", false); show("themeBtn", false); show("login", true);
  }

  function enterApp() { show("login", false); show("app", true); show("themeBtn", true); loadMine(); loadVideos(); }

  function field(o, k) { return o[k] != null ? o[k] : o[k.charAt(0).toLowerCase() + k.slice(1)]; }

  function loadMine() {
    api("/YouTubeFast/User/Channels?userId=" + encodeURIComponent(userId)).then(function (list) {
      var box = document.getElementById("mine"); box.innerHTML = "";
      var n = list.length;
      document.getElementById("count").textContent = n + (n > 1 ? " chaînes" : " chaîne");
      if (!n) { box.innerHTML = '<p style="color:var(--text-dim); font-size:14px;">Aucune chaîne. Recherchez ci-dessus pour en ajouter.</p>'; return; }
      list.forEach(function (c, idx) {
        var name = field(c, "Name"), cid = field(c, "ChannelId"), ex = field(c, "ExcludeShorts");
        var thumb = field(c, "Thumbnail");
        var card = document.createElement("div");
        card.className = "panel rowline"; card.style.padding = "14px 16px";
        card.appendChild(avatar(name, idx + 2, thumb));
        var nm = document.createElement("span"); nm.className = "name"; nm.style.fontSize = "15px"; nm.textContent = name;
        card.appendChild(nm);

        var sh = document.createElement("button"); sh.className = "shorts" + (ex ? " on" : "");
        sh.innerHTML = '<span class="dot"></span>Exclure les Shorts';
        sh.addEventListener("click", function () {
          ex = !ex; sh.className = "shorts" + (ex ? " on" : "");
          api("/YouTubeFast/User/ToggleShorts", { method: "POST", body: JSON.stringify({ userId: userId, channelId: cid, excludeShorts: ex }) });
        });
        card.appendChild(sh);

        var rm = document.createElement("button"); rm.className = "remove"; rm.textContent = "Retirer";
        rm.addEventListener("click", function () {
          api("/YouTubeFast/User/Remove", { method: "POST", body: JSON.stringify({ userId: userId, channelId: cid }) }).then(loadMine);
        });
        card.appendChild(rm);
        box.appendChild(card);
      });
    });
  }

  function search() {
    var q = document.getElementById("q").value.trim(); if (q.length < 2) return;
    var box = document.getElementById("results");
    box.innerHTML = '<p style="color:var(--text-dim); font-size:14px;">Recherche…</p>';
    api("/YouTubeFast/User/Search", { method: "POST", body: JSON.stringify({ query: q }) }).then(function (list) {
      box.innerHTML = "";
      if (!list.length) { box.innerHTML = '<p style="color:var(--text-dim); font-size:14px;">Aucune chaîne trouvée.</p>'; return; }
      list.forEach(function (c, i) {
        var name = field(c, "Name"), cid = field(c, "ChannelId");
        var thumb = field(c, "Thumbnail");
        var row = document.createElement("div");
        row.className = "rowline"; row.style.cssText = "padding:10px 12px; border-radius:13px; background:var(--bg-elev2); border:1px solid transparent;";
        row.appendChild(avatar(name, i, thumb));
        var nm = document.createElement("span"); nm.className = "name"; nm.style.fontSize = "14.5px"; nm.textContent = name;
        row.appendChild(nm);
        var add = document.createElement("button"); add.className = "btn-accent"; add.style.cssText = "padding:8px 16px; font-size:13px;"; add.textContent = "Ajouter";
        add.addEventListener("click", function () {
          api("/YouTubeFast/User/Add", { method: "POST", body: JSON.stringify({ userId: userId, userName: userName, channelId: cid, name: name, thumbnail: thumb }) })
            .then(function () { row.remove(); loadMine(); });
        });
        row.appendChild(add);
        box.appendChild(row);
      });
    }).catch(function (e) { box.innerHTML = '<p style="color:var(--text-dim); font-size:14px;">Recherche échouée (' + (e && e.message) + ').</p>'; });
  }

  function switchTab(which) {
    var isVideo = which === "video";
    show("tabVideo", isVideo);
    show("tabChaine", !isVideo);
    document.getElementById("tabBtnVideo").classList.toggle("active", isVideo);
    document.getElementById("tabBtnChaine").classList.toggle("active", !isVideo);
  }

  function loadVideos() {
    api("/YouTubeFast/User/Videos?userId=" + encodeURIComponent(userId)).then(function (list) {
      var box = document.getElementById("videos"); box.innerHTML = "";
      var n = list.length;
      document.getElementById("vcount").textContent = n + (n > 1 ? " vidéos" : " vidéo");
      if (!n) { box.innerHTML = '<p style="color:var(--text-dim); font-size:14px;">Aucune vidéo. Collez un lien ci-dessus pour en ajouter.</p>'; return; }
      list.forEach(function (v, idx) {
        var name = field(v, "Title"), vid = field(v, "VideoId"), thumb = field(v, "Thumbnail");
        var card = document.createElement("div");
        card.className = "panel rowline"; card.style.padding = "14px 16px";
        card.appendChild(avatar(name, idx + 1, thumb));
        var nm = document.createElement("span"); nm.className = "name"; nm.style.fontSize = "15px"; nm.textContent = name;
        card.appendChild(nm);

        var rm = document.createElement("button"); rm.className = "remove"; rm.textContent = "Retirer";
        rm.addEventListener("click", function () {
          api("/YouTubeFast/User/RemoveVideo", { method: "POST", body: JSON.stringify({ userId: userId, videoId: vid }) }).then(loadVideos);
        });
        card.appendChild(rm);
        box.appendChild(card);
      });
    });
  }

  function addVideo() {
    var url = document.getElementById("vq").value.trim();
    var msg = document.getElementById("vmsg");
    if (!url) return;
    msg.style.color = "var(--text-dim)"; msg.textContent = "Ajout…";
    api("/YouTubeFast/User/AddVideo", { method: "POST", body: JSON.stringify({ userId: userId, userName: userName, url: url }) })
      .then(function (r) {
        var status = r && field(r, "Status");
        if (status === "exists") {
          msg.style.color = "var(--text-dim)";
          msg.textContent = "Cette vidéo est déjà dans votre bibliothèque.";
        } else if (status === "short") {
          msg.style.color = "#e0738a";
          msg.textContent = "C'est un Short — non ajouté (l'application exclut les Shorts).";
        } else {
          msg.textContent = ""; document.getElementById("vq").value = ""; toast("Vidéo ajoutée.");
        }
        loadVideos();
      })
      .catch(function () {
        msg.style.color = "#e0738a";
        msg.textContent = "Échec de l'ajout. Vérifiez que le lien est une vidéo YouTube valide.";
      });
  }

  function toast(msg) {
    var t = document.createElement("div"); t.className = "toast"; t.textContent = msg;
    document.body.appendChild(t); setTimeout(function () { t.remove(); }, 3500);
  }
  function showModal(on) { show("confirmModal", on); }

  function doSync() {
    var icon = document.getElementById("syncIcon"); icon.classList.add("spin");
    api("/YouTubeFast/User/Sync", { method: "POST" }).then(function () {
      var d = new Date();
      var t = String(d.getHours()).padStart(2,"0") + ":" + String(d.getMinutes()).padStart(2,"0");
      document.getElementById("syncStatus").textContent = "Dernière synchro à " + t;
      toast("Synchronisation lancée.");
    }).catch(function (e) { toast("Échec (" + (e && e.message) + ")."); })
      .then(function () { setTimeout(function(){ icon.classList.remove("spin"); }, 1200); });
  }

  document.getElementById("loginBtn").addEventListener("click", login);
  document.getElementById("logoutBtn").addEventListener("click", logout);
  document.getElementById("searchBtn").addEventListener("click", search);
  document.getElementById("tabBtnChaine").addEventListener("click", function () { switchTab("chaine"); });
  document.getElementById("tabBtnVideo").addEventListener("click", function () { switchTab("video"); });
  document.getElementById("addVideoBtn").addEventListener("click", addVideo);
  document.getElementById("vq").addEventListener("keydown", function (e) { if (e.key === "Enter") addVideo(); });
  document.getElementById("themeBtn").addEventListener("click", function () {
    applyTheme(document.documentElement.classList.contains("light") ? "dark" : "light");
  });
  document.getElementById("syncBtn").addEventListener("click", function () { showModal(true); });
  document.getElementById("confirmNo").addEventListener("click", function () { showModal(false); });
  document.getElementById("confirmYes").addEventListener("click", function () { showModal(false); doSync(); });
  document.getElementById("q").addEventListener("keydown", function (e) { if (e.key === "Enter") search(); });
  document.getElementById("p").addEventListener("keydown", function (e) { if (e.key === "Enter") login(); });

  applyTheme(localStorage.getItem("ytf_theme") || "dark");
  if (token && userId) { enterApp(); }
})();
</script>
</body>
</html>
""";
}
