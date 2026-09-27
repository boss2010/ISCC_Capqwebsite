const fs = require('fs');
const path = require('path');
const { chromium } = require('C:/Users/pc/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root = path.resolve('Capqwebsite');
let layout = fs.readFileSync(path.join(root, 'Views/Shared/_Layout.cshtml'), 'utf8').replace(/@\*[\s\S]*?\*@/g, '');
const header = layout.slice(layout.indexOf('<header'), layout.indexOf('</header>') + 9);
const navStart = header.indexOf('<div class="navbar-nav');
const navEnd = header.indexOf('else if');
const publicHeader = header.slice(0, header.indexOf('@if')) + header.slice(navStart, navEnd).replace(/\}\s*$/, '') + '</div></nav></div></header>';
const home = fs.readFileSync(path.join(root, 'Views/Home/Index.cshtml'), 'utf8');
const sections = home.slice(home.indexOf('<section'), home.indexOf('@* ===== ABOUT'));
const html = '<!doctype html><html lang="ar" dir="rtl"><head><meta charset="utf-8"><link rel="stylesheet" href="/fontawesome-free-5.15.4-web/css/all.min.css"><meta name="viewport" content="width=device-width, initial-scale=1"><link rel="stylesheet" href="/css/styleESLAM.css"><link rel="stylesheet" href="/css/styles.css"><link rel="stylesheet" href="/css/portal-responsive.css"></head><body class="gov-portal">' + publicHeader + '<main class="gov-main">' + sections + '</main><script src="/js/bootstrap.bundle5.min.js"></script></body></html>';
(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  const page = await browser.newPage();
  await page.route('http://preview.local/**', async route => {
    const url = new URL(route.request().url());
    if (url.pathname === '/') return route.fulfill({contentType:'text/html', body:html.replace(/~\//g,'/')});
    const file = path.join(root, 'wwwroot', decodeURIComponent(url.pathname));
    if (!fs.existsSync(file)) return route.fulfill({status:404,body:''});
    const types = {'.css':'text/css','.js':'application/javascript','.png':'image/png','.jpg':'image/jpeg','.ttf':'font/ttf'};
    return route.fulfill({body:fs.readFileSync(file),contentType:types[path.extname(file)] || 'application/octet-stream'});
  });
  for (const width of [320,375,390,768,1024,1440]) {
    await page.setViewportSize({width,height:850});
    await page.goto('http://preview.local/');
    await page.evaluate(()=>document.fonts.ready);
    const overflow = await page.evaluate(()=>document.documentElement.scrollWidth > innerWidth);
    if (overflow) throw new Error('Horizontal overflow at '+width);
    if (width < 1200) {
      await page.locator('.navbar-toggler').click();
      await page.waitForTimeout(400);
      if (!await page.locator('#navbarCollapse').isVisible()) throw new Error('Menu not visible');
      if (await page.locator('#navbarCollapse .services:visible').count() !== 2) throw new Error('Missing service links');
      await page.locator('.dropdown-toggle').first().click();
      if (!await page.locator('.dropdown-menu').first().isVisible()) throw new Error('Dropdown not visible');
      if (await page.evaluate(()=>document.documentElement.scrollWidth > innerWidth)) throw new Error('Menu overflow');
      if (width===390) await page.screenshot({path:'tmp/responsive-menu.png'});
      await page.locator('.navbar-toggler').click();
      await page.waitForTimeout(400);
      if (await page.locator('#navbarCollapse').isVisible()) throw new Error('Menu did not close');
    }
    if (width===390) await page.screenshot({path:'tmp/responsive-home.png',fullPage:true});
    console.log(width+': no overflow; navigation passed');
  }
  await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});

