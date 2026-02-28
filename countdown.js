// Default target date
const DEFAULT_TARGET_DATE = '2028-10-01T00:00:00';

// --- Target date ---
function loadTargetDate() {
    return localStorage.getItem('targetDate') || DEFAULT_TARGET_DATE;
}

function saveTargetDate(dateStr) {
    localStorage.setItem('targetDate', dateStr);
}

function formatDateDisplay(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleString('en-US', {
        year: 'numeric', month: 'long', day: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit'
    });
}

let targetDateStr = loadTargetDate();
let targetDate = new Date(targetDateStr).getTime();

function updateDateDisplay() {
    const display = document.getElementById('target-date-display');
    const title = document.getElementById('countdown-title');
    const formatted = formatDateDisplay(targetDateStr);
    display.textContent = 'Target Date: ' + formatted;
    title.textContent = 'Countdown to ' + new Date(targetDateStr).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
}

// --- Countdown ---
function updateCountdown() {
    const now = new Date().getTime();
    const distance = targetDate - now;

    if (distance < 0) {
        document.getElementById('days').textContent = '0';
        document.getElementById('hours').textContent = '0';
        document.getElementById('minutes').textContent = '0';
        document.getElementById('seconds').textContent = '0';
        return;
    }

    const days = Math.floor(distance / (1000 * 60 * 60 * 24));
    const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((distance % (1000 * 60)) / 1000);

    document.getElementById('days').textContent = days;
    document.getElementById('hours').textContent = hours.toString().padStart(2, '0');
    document.getElementById('minutes').textContent = minutes.toString().padStart(2, '0');
    document.getElementById('seconds').textContent = seconds.toString().padStart(2, '0');
}

// --- Slideshow ---
const DEFAULT_IMAGE = 'us.jpg';
let slideshowImages = [];
let currentSlide = 0;
let slideshowInterval = null;

function loadSlideshowImages() {
    try {
        const stored = localStorage.getItem('slideshowImages');
        return stored ? JSON.parse(stored) : [];
    } catch (e) {
        return [];
    }
}

function saveSlideshowImages(images) {
    try {
        localStorage.setItem('slideshowImages', JSON.stringify(images));
    } catch (e) {
        alert('Could not save images: storage limit may be exceeded. Try using fewer or smaller images.');
    }
}

function updateSlideshowUI() {
    const img = document.getElementById('slideshow-image');
    const controls = document.getElementById('slideshow-controls');
    const indicator = document.getElementById('slide-indicator');

    if (slideshowImages.length === 0) {
        img.src = DEFAULT_IMAGE;
        controls.style.display = 'none';
        clearInterval(slideshowInterval);
        slideshowInterval = null;
    } else {
        img.src = slideshowImages[currentSlide];
        controls.style.display = slideshowImages.length > 1 ? 'flex' : 'none';
        indicator.textContent = (currentSlide + 1) + ' / ' + slideshowImages.length;

        if (slideshowImages.length > 1 && !slideshowInterval) {
            slideshowInterval = setInterval(function() {
                currentSlide = (currentSlide + 1) % slideshowImages.length;
                updateSlideshowUI();
            }, 5000);
        }
    }
}

function readFilesAsDataURLs(files, callback) {
    const results = [];
    let remaining = files.length;
    if (remaining === 0) { callback([]); return; }
    Array.from(files).forEach(function(file, i) {
        const reader = new FileReader();
        reader.onload = function(e) {
            results[i] = e.target.result;
            remaining--;
            if (remaining === 0) callback(results);
        };
        reader.readAsDataURL(file);
    });
}

// --- Settings panel ---
document.getElementById('settings-toggle').addEventListener('click', function() {
    const panel = document.getElementById('settings-panel');
    const expanded = !panel.hidden;
    panel.hidden = expanded;
    this.setAttribute('aria-expanded', String(!expanded));
});

document.getElementById('apply-date-btn').addEventListener('click', function() {
    const input = document.getElementById('target-date-input').value;
    if (!input) { alert('Please select a date and time.'); return; }
    if (new Date(input).getTime() <= Date.now()) {
        if (!confirm('The selected date is in the past. The countdown will show all zeros. Continue?')) return;
    }
    targetDateStr = input;
    targetDate = new Date(targetDateStr).getTime();
    saveTargetDate(targetDateStr);
    updateDateDisplay();
    updateCountdown();
});

document.getElementById('photo-upload').addEventListener('change', function(e) {
    const files = e.target.files;
    if (!files || files.length === 0) return;
    readFilesAsDataURLs(files, function(dataURLs) {
        slideshowImages = slideshowImages.concat(dataURLs);
        currentSlide = 0;
        clearInterval(slideshowInterval);
        slideshowInterval = null;
        saveSlideshowImages(slideshowImages);
        updateSlideshowUI();
    });
    e.target.value = '';
});

document.getElementById('clear-photos-btn').addEventListener('click', function() {
    slideshowImages = [];
    currentSlide = 0;
    clearInterval(slideshowInterval);
    slideshowInterval = null;
    localStorage.removeItem('slideshowImages');
    updateSlideshowUI();
});

function navigateToSlide(newIndex) {
    if (slideshowImages.length === 0) return;
    currentSlide = (newIndex + slideshowImages.length) % slideshowImages.length;
    clearInterval(slideshowInterval);
    slideshowInterval = null;
    updateSlideshowUI();
}

document.getElementById('prev-btn').addEventListener('click', function() {
    navigateToSlide(currentSlide - 1);
});

document.getElementById('next-btn').addEventListener('click', function() {
    navigateToSlide(currentSlide + 1);
});

// --- Init ---
(function init() {
    // Pre-fill date input with stored value
    const inputEl = document.getElementById('target-date-input');
    // datetime-local value format: YYYY-MM-DDTHH:MM
    inputEl.value = targetDateStr.slice(0, 16);

    updateDateDisplay();
    updateCountdown();
    setInterval(updateCountdown, 1000);

    slideshowImages = loadSlideshowImages();
    updateSlideshowUI();
})();

