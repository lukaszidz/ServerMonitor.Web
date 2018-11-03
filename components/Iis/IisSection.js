import React, { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import ReactTable from 'react-table';
import 'react-table/react-table.css';
import * as actions from '../../actions/actions';
import AppPoolList from './AppPoolList';
import NoteControl from './NoteControl';
import ActionsButtons from './ActionsButtons';
import WhitelistButton from './WhitelistButton';

const flatten = list =>
  list.reduce((a, b) => a.concat(Array.isArray(b) ? flatten(b) : b), []);

class IisSection extends Component {
  componentDidMount() {
    this.props.getIis();
  }
  render() {
    return (
      <React.Fragment>
        <ReactTable
          showPagination={false}
          minRows={1}
          data={this.props.iis.data}
          columns={[
            {
              Header: 'Name',
              accessor: 'name',
              Cell: row => (
                <a target="_blank" href={row.original.url}>
                  {row.original.name}
                </a>
              )
            },
            {
              Header: 'State',
              accessor: 'running',
              Cell: row => (
                <font className={`state-${row.value ? 'started' : 'stopped'}`}>
                  {row.value ? 'Started' : 'Stopped'}
                </font>
              ),
              width: 100
            },
            {
              Header: 'Note',
              accessor: 'note',
              Cell: row => <NoteControl {...row.original} />,
              width: 400
            },
            {
              Header: 'Reserved',
              accessor: 'whitelisted',
              Cell: row => (
                <WhitelistButton
                  {...row.original}
                  click={this.props.whitelist}
                />
              ),
              width: 100
            },
            {
              Header: 'Action',
              accessor: 'x',
              Cell: row => <ActionsButtons {...row.original} />,
              width: 100
            }
          ]}
          SubComponent={row => <AppPoolList items={row.original.apps} />}
        />
      </React.Fragment>
    );
  }
}

IisSection.propTypes = {
  getIis: PropTypes.func.isRequired,
  iis: PropTypes.shape({
    data: PropTypes.arrayOf(PropTypes.object).isRequired,
    columns: PropTypes.arrayOf(PropTypes.object).isRequired,
    loading: PropTypes.bool.isRequired
  }).isRequired,
  whitelist: PropTypes.func.isRequired
};

const mapStateToProps = state => ({
  iis: state.table
});

const mapDispatchToProps = dispatch => ({
  getIis: () => dispatch(actions.getIisAction()),
  whitelist: (item) => {
    const data = {
      appPools: flatten(item.apps.map(x => x.children)),
      condition: item.whitelisted
    };
    dispatch(actions.whitelistApp(data));
  }
});

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(IisSection);
