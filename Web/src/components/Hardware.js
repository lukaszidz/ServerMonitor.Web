import React, { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import PageVisibility from 'react-page-visibility';
import { Tabs } from 'antd';
import { getHardwareAction } from '../actions/actions';
import HardwareItem from './HardwareItem';

const { TabPane } = Tabs;
const keyPropName = "key";

class Hardware extends Component {
  constructor(props) {
    super(props);
    this.state = {
      visible: true
    };
    this.handleVisibilityChange = this.handleVisibilityChange.bind(this);
    this.dispatchChange = this.dispatchChange.bind(this);
  }

  componentDidMount() {
    setInterval(this.dispatchChange, 5000);
  }

  handleVisibilityChange(visible) {
    this.setState({ visible });
  }

  dispatchChange() {
    if (this.state.visible) {
      this.props.dispatch(getHardwareAction());
    }
  }

  render() {
    const machines = this.props.data ? groupBy(this.props.data.flatMap(x => x), keyPropName) : [];
    const machinesKeys = Object.keys(machines);
    const components = machinesKeys.map(x => 
        <TabPane tab={x} key={x}>
          <HardwareItem item={
            groupBy(machines[x].reduce(function(rd, v) {
              rd.push(v.data);
              return rd;
          }, []).flatMap(x => x), keyPropName)} />
        </TabPane>
    );
    return (
      <PageVisibility onChange={this.handleVisibilityChange}>
        <Tabs tabPosition="left" style={{ padding: 5 }} >
          {components}
        </Tabs>
      </PageVisibility>
    );
  }
}

const groupBy = function(xs, key) {
  return xs.reduce(function(rv, x) {
    (rv[x[key]] = rv[x[key]] || []).push(x);
    return rv;
  }, {});
};

const mapStateToProps = state => ({
  data: state.hardware
});

Hardware.propTypes = {
  dispatch: PropTypes.func.isRequired,
  data: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default connect(mapStateToProps)(Hardware);
