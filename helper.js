const engagements = require('./_data/engagements.json')

function getEngagementsList() {
  return [
    ...engagements.past.map(year => year.engagements).flat(),
    ...engagements.upcoming
  ]
}

exports.getEngagementsList = getEngagementsList