const tabs = Array.from(document.querySelectorAll('.tab'));
const nav = document.getElementById('tabsNav')
let previousWasMobile = false
const minWindowWidth = 455

window.addEventListener('resize', handleTabResize)
handleTabResize()

function selectTab(tab, event) {
  const navItems = Array.from(nav.querySelectorAll('li'))
  tabs.forEach(tab => tab.classList.remove('selected'))
  navItems.forEach(item => item.classList.remove('selected'))

  document.getElementById(tab).classList.add('selected')
  const originalNode = event.srcElement.parentElement
  originalNode.classList.add('selected')
  
  if (!isMobile()) { return }
  const clone = originalNode.cloneNode(true)
  nav.querySelector('ul').appendChild(clone)
  originalNode.remove()
}

function handleTabResize() {
  console.log('resize')
  if ((isMobile() && previousWasMobile) || (!isMobile() && !previousWasMobile)) { return }
  previousWasMobile = isMobile()
  const navItems = Array.from(nav.querySelectorAll('li'))
  const ul = nav.querySelector('ul')
  if (isMobile()) {
    navItems.sort((a,b) => {
      if (a.classList.contains('selected')) { return 1 }
      if (b.classList.contains('selected')) { return -1 }
      return Number(b.getAttribute('data-order')) - Number(a.getAttribute('data-order'))
    })
  } else {
    // else desktop
    navItems.sort((a,b) => {
      return Number(a.getAttribute('data-order')) - Number(b.getAttribute('data-order'))
    })
  }

  ul.innerHTML = null
  navItems.forEach(item => ul.appendChild(item))
}

function isMobile() {
  return window.innerWidth <= minWindowWidth
}