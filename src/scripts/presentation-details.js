const tabs = Array.from(document.querySelectorAll('.tab'));
const accordion = document.querySelector('.accordion')
const accordionPanels = Array.from(accordion.querySelectorAll('.panel'))
const nav = document.getElementById('tabsNav')
let previousWasMobile = false
const minWindowWidth = 455;

(function() {
  'use strict'
  accordionInit()
  tabInit()

  window.addEventListener('resize', () => { accordionInit(); tabInit()})
  const accordionHeaderButtons = Array.from(accordion.querySelectorAll('.panel > h2 > button'))
  accordionHeaderButtons.forEach((button, index) => 
    button.addEventListener('keydown', (event) => {
      const keys = [ 'ArrowDown', 'ArrowUp', 'Home', 'End']
      if (!keys.includes(event.code)) { return; }
      event.preventDefault()
      switch(event.code) {
        case 'ArrowDown':
          // we are the last button
          if (index === accordionHeaderButtons.length -1) {
            accordionHeaderButtons[0].focus()
            return
          }
          accordionHeaderButtons[index+1].focus()
          break
        case 'ArrowUp':
          // we are the first button
          if (index === 0) {
            accordionHeaderButtons[accordionHeaderButtons.length - 1].focus()
            return
          }
          accordionHeaderButtons[index-1].focus()
          break
        case 'Home':
          accordionHeaderButtons[0].focus()
          break;
        case 'End':
          accordionHeaderButtons[accordionHeaderButtons.length - 1].focus()
          break;
      }
    })
  )
  const tabButtons = Array.from(document.querySelectorAll('#tabsNav button'))
  tabButtons.forEach((button, index) => {
    button.addEventListener('keydown', (event) => {
      console.log(event)
      const keys = [ 'ArrowRight', 'ArrowLeft', 'Home', 'End']
      if (!keys.includes(event.code)) { return; }
      event.preventDefault()
      switch(event.code) {
        case 'ArrowRight': 
          if (index === tabButtons.length - 1) { tabButtons[0].focus(); return }
          tabButtons[index + 1].focus()
          break;
        case 'ArrowLeft':
          if (index === 0) { tabButtons[tabButtons.length - 1].focus(); return }
          tabButtons[index - 1].focus()
          break;
        case 'Home': tabButtons[0].focus(); break;
        case 'End': tabButtons[tabButtons.length - 1].focus(); break;
      }
    })
  })
})();

function accordionInit() {
  if (!isMobile()) return;
  accordionPanels.forEach(panel => {
    handleAccordionPanel(panel, Array.from(panel.classList).includes('expanded')) 
  })
}

function tabInit() {
  if (isMobile()) { return; }
  const panelContainer = document.querySelector('.panel-container')
  console.log(tabs.map(t => t.offsetHeight))
  const height = tabs.map(t => t.offsetHeight).sort((a, b) => b - a)[0]
  panelContainer.style.height = `calc(${height}px + 1rem)`
}

function handleAccordionPanel(panel, opening) {
  const body = panel.querySelector('.panel-body')
  const button = panel.querySelector('h2 > button')
  const focusableElements = Array.from(body.querySelectorAll('a,button'))
  if (opening) {
    button.setAttribute('aria-expanded', 'true')
    body.setAttribute('aria-hidden', 'false')
    focusableElements.forEach(elem => {
      elem.removeAttribute('tabindex')
    })
    const panelHeight = body.offsetHeight
    panel.style.maxHeight = `calc(2.5rem + ${panelHeight}px)`
  } else {
    button.setAttribute('aria-expanded', 'false')
    body.setAttribute('aria-hidden', true)
    focusableElements.forEach(elem => {
      elem.setAttribute('tabindex', '-1')
    })
    panel.style.maxHeight = '2.5rem'
  }
}

function selectTab(tab, event) {
  tabInit()
  const navItems = Array.from(nav.querySelectorAll('button'))
  tabs.forEach(_tab => _tab.classList.remove('selected'))
  navItems.forEach(item => {
    item.classList.remove('selected')
    item.setAttribute('tabindex', -1)
    item.setAttribute('aria-selected', false)
  })

  document.getElementById(tab).classList.add('selected')
  const originalNode = event.srcElement.parentElement
  originalNode.classList.add('selected')
  event.srcElement.classList.add('selected')
  event.srcElement.setAttribute('tabindex', 0)
  event.srcElement.setAttribute('aria-selected', true)
}

function togglePanel(event) {
  console.log(event)
  const panel = event.srcElement.closest('.panel')
  handleAccordionPanel(panel, !Array.from(panel.classList).includes('expanded'))
  panel.classList.toggle('expanded')
}

function isMobile() {
  return window.innerWidth <= minWindowWidth
}