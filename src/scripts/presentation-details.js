const tabs = Array.from(document.querySelectorAll('.tab'));
const accordion = document.querySelector('.accordion')
const accordionPanels = Array.from(accordion.querySelectorAll('.panel'))
const nav = document.getElementById('tabsNav')
let previousWasMobile = false
const minWindowWidth = 455;

(function() {
  'use strict'
  window.addEventListener('resize', () => {
    if (!isMobile) return;
    accordionPanels.forEach(panel => {
      handleAccordionPanel(panel, Array.from(panel.classList).includes('expanded')) 
    })
  })
})();

function handleAccordionPanel(panel, opening) {
  const body = panel.querySelector('.panel-body')
  if (opening) {
    const panelHeight = body.offsetHeight
    panel.style.maxHeight = `calc(2.5rem + ${panelHeight}px)`
  } else {
    panel.style.maxHeight = '2.5rem'
  }
}

function selectTab(tab, event) {
  const navItems = Array.from(nav.querySelectorAll('li'))
  tabs.forEach(tab => tab.classList.remove('selected'))
  navItems.forEach(item => item.classList.remove('selected'))

  document.getElementById(tab).classList.add('selected')
  const originalNode = event.srcElement.parentElement
  originalNode.classList.add('selected')
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