// Mobile nav toggle
document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('navToggle');
    var links = document.getElementById('navLinks');
    if (toggle && links) {
        toggle.addEventListener('click', function () { links.classList.toggle('open'); });
    }
});

// ---------- AJAX: Live scores (polls every 10s) ----------
function startLiveScores(tournamentId, containerId) {
    var container = document.getElementById(containerId);
    if (!container) return;

    function render(matches) {
        if (!matches.length) {
            container.innerHTML = '<div class="kos-empty">No live or finished matches yet.</div>';
            return;
        }
        container.innerHTML = matches.map(function (m) {
            var live = m.status === 'Live'
                ? '<span class="kos-pill kos-pill-live">LIVE</span>'
                : '<span class="kos-pill kos-pill-gray">' + m.status + '</span>';
            return '<div class="kos-card"><div class="kos-card-body">' +
                '<div class="kos-between"><span class="kos-meta">Round ' + m.round + '</span>' + live + '</div>' +
                '<div class="kos-between kos-mt" style="margin-top:12px">' +
                '<strong>' + m.teamA + '</strong>' +
                '<span class="kos-rating-badge">' + m.scoreA + ' : ' + m.scoreB + '</span>' +
                '<strong>' + m.teamB + '</strong></div></div></div>';
        }).join('');
    }

    function poll() {
        fetch('/api/matches/live/' + tournamentId)
            .then(function (r) { return r.json(); })
            .then(render)
            .catch(function () { /* network hiccup - keep last render */ });
    }
    poll();
    setInterval(poll, 10000);
}

// ---------- AJAX: Bracket (dynamic load) ----------
function loadBracket(tournamentId, containerId) {
    var container = document.getElementById(containerId);
    if (!container) return;
    fetch('/api/matches/bracket/' + tournamentId)
        .then(function (r) { return r.json(); })
        .then(function (rounds) {
            if (!rounds.length) { container.innerHTML = '<div class="kos-empty">Bracket not generated yet.</div>'; return; }
            container.innerHTML = rounds.map(function (rd) {
                return '<div class="kos-bracket-round"><h3 style="font-size:1rem;color:#6b7280">Round ' + rd.round + '</h3>' +
                    rd.matches.map(function (m) {
                        var aWin = m.winner && m.winner === m.teamA ? ' winner' : '';
                        var bWin = m.winner && m.winner === m.teamB ? ' winner' : '';
                        return '<div class="kos-bracket-match">' +
                            '<div class="kos-bracket-team' + aWin + '"><span>' + m.teamA + '</span><span>' + m.scoreA + '</span></div>' +
                            '<div class="kos-bracket-team' + bWin + '"><span>' + m.teamB + '</span><span>' + m.scoreB + '</span></div>' +
                            '</div>';
                    }).join('') + '</div>';
            }).join('');
        })
        .catch(function () { container.innerHTML = '<div class="kos-empty">Could not load bracket.</div>'; });
}

// ---------- AJAX: MVP voting (no page refresh) ----------
function voteMvp(tournamentId, playerId, btn) {
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    fetch('/api/votes/mvp', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token ? token.value : ''
        },
        body: JSON.stringify({ tournamentId: tournamentId, playerId: playerId })
    })
        .then(function (r) { return r.json(); })
        .then(function (res) {
            var msg = document.getElementById('voteMsg');
            if (msg) { msg.textContent = res.message; msg.style.display = 'block'; }
            var counter = document.getElementById('voteCount-' + playerId);
            if (counter && res.count !== undefined) counter.textContent = res.count;
            if (btn && res.success) { btn.disabled = true; btn.textContent = 'Voted'; }
        })
        .catch(function () { alert('Vote failed. Please sign in and try again.'); });
}

// ---------- AJAX: Live search (teams/players) ----------
function liveSearch(inputId, url, containerId, renderRow) {
    var input = document.getElementById(inputId);
    var container = document.getElementById(containerId);
    if (!input || !container) return;
    var timer = null;
    input.addEventListener('input', function () {
        clearTimeout(timer);
        timer = setTimeout(function () {
            fetch(url + '?term=' + encodeURIComponent(input.value))
                .then(function (r) { return r.json(); })
                .then(function (items) {
                    container.innerHTML = items.length
                        ? items.map(renderRow).join('')
                        : '<div class="kos-empty">No results found.</div>';
                })
                .catch(function () { });
        }, 250);
    });
}
